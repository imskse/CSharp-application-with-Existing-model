using NumSharp;
using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Tensorflow;
using Tensorflow.Keras.Utils;
using TensorFlowNET.Examples.Utility;
using static Tensorflow.Binding;
using System.Drawing.Imaging;


namespace PytCshv4
{
    public partial class Form1 : Form
    {
        public float MIN_SCORE = 0.5f;

        string modelDir = "ssd_mobilenet_v2_coco_2018_03_29";
        string imageDir = "images";
        string pbFile = "frozen_inference_graph.pb";
        string _FileName = "";
        private Bitmap MyImage;
        public Form1()
        {
            InitializeComponent();

        }
        public bool Run()
        {
            tf.compat.v1.disable_eager_execution();

            PrepareData();

            Predict();

            return true;
        }

        private void PrepareData()
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
        public Graph ImportGraph()
        {
            var graph = new Graph().as_default();
            graph.Import(Path.Combine(modelDir, pbFile));

            return graph;
        }
        public void Predict()
        {
            // read in the input image
            var imgArr = ReadTensorFromImageFile(_FileName);

            var graph = ImportGraph();

            using (var sess = tf.Session(graph))
            {
                Tensor tensorNum = graph.OperationByName("num_detections");
                Tensor tensorBoxes = graph.OperationByName("detection_boxes");
                Tensor tensorScores = graph.OperationByName("detection_scores");
                Tensor tensorClasses = graph.OperationByName("detection_classes");
                Tensor imgTensor = graph.OperationByName("image_tensor");
                Tensor[] outTensorArr = new Tensor[] { tensorNum, tensorBoxes, tensorScores, tensorClasses };

                var results = sess.run(outTensorArr, new FeedItem(imgTensor, imgArr));

                buildOutputImage(results);
            }
        }

        private NDArray ReadTensorFromImageFile(string file_name)
        {
            var graph = tf.Graph().as_default();

            var file_reader = tf.io.read_file(file_name, "file_reader");
            var decodeJpeg = tf.image.decode_jpeg(file_reader, channels: 3, name: "DecodeJpeg");
            var casted = tf.cast(decodeJpeg, TF_DataType.TF_UINT8);
            var dims_expander = tf.expand_dims(casted, 0);

            using (var sess = tf.Session(graph))
                return sess.run(dims_expander);
        }
        private void buildOutputImage(NDArray[] resultArr)
        {
            // get pbtxt items
            PbtxtItems pbTxtItems = PbtxtParser.ParsePbtxtFile(Path.Combine(modelDir, "mscoco_label_map.pbtxt"));

            // get bitmap
            Bitmap bitmap = new Bitmap(_FileName);

            var scores = resultArr[2].AsIterator<float>();
            var boxes = resultArr[1].GetData<float>();
            var id = np.squeeze(resultArr[3]).GetData<float>();
            for (int i = 0; i < scores.size; i++)
            {
                float score = scores.MoveNext();
                if (score > MIN_SCORE)
                {
                    float top = boxes[i * 4] * bitmap.Height;
                    float left = boxes[i * 4 + 1] * bitmap.Width;
                    float bottom = boxes[i * 4 + 2] * bitmap.Height;
                    float right = boxes[i * 4 + 3] * bitmap.Width;

                    Rectangle rect = new Rectangle()
                    {
                        X = (int)left,
                        Y = (int)top,
                        Width = (int)(right - left),
                        Height = (int)(bottom - top)
                    };

                    string name = pbTxtItems.items.Where(w => w.id == id[i]).Select(s => s.display_name).FirstOrDefault();
                    label1.Text = name.ToString();
                    drawObjectOnBitmap(bitmap, rect, score, name,i+1);
                }
            }

            string path = Path.Combine(imageDir, "output.jpg");
            pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox2.Image = (Image)bitmap;
            //pictureBox2.Image = Image.FromStream(bitmap.Clone());
            //bitmap.Save(path);
            //Console.WriteLine($"Processed image is saved as {path}");
        }

        private void drawObjectOnBitmap(Bitmap bmp, Rectangle rect, float score, string name,int Number)
        {
            using (Graphics graphic = Graphics.FromImage(bmp))
            {
                graphic.SmoothingMode = SmoothingMode.AntiAlias;

                using (Pen pen = new Pen(Color.Red, 2))
                {
                    graphic.DrawRectangle(pen, rect);

                    System.Drawing.Point p = new System.Drawing.Point(rect.Right + 5, rect.Top + 5);
                    string text = string.Format("{0}:{1}%:{2}", name, (int)(score * 100),Number);
                    graphic.DrawString(text, new Font("Verdana", 14), Brushes.Red, p);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "Image Files(*.jpg; *.jpeg; *.gif; *.bmp)|*.jpg; *.jpeg; *.gif; *.bmp";
            if (open.ShowDialog() == DialogResult.OK)
            {
                Bitmap img = new Bitmap(open.FileName);
                ShowMyImage(Path.Combine(Path.GetDirectoryName(open.FileName), open.FileName.ToString()), img.Height, img.Width);
            }
        }
        public void ShowMyImage(String fileToDisplay, int xSize, int ySize)
        {
            if (MyImage != null) MyImage.Dispose();
            _FileName = fileToDisplay;
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            MyImage = new Bitmap(fileToDisplay);
            //pictureBox1.ClientSize = new Size(xSize, ySize);
            pictureBox1.Image = (Image)MyImage;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //if (true == Run())
            //    MessageBox.Show("DONE", "Error");
            Run();
        }

        private void button2_Click_1(object sender, EventArgs e)
        {

        }
    }

}
