using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TensorFlowNET.Examples.Utility
{
    public class CoNLLDataset
    {
        static Dictionary<string, int> vocab_chars;
        static Dictionary<string, int> vocab_words;
        static Dictionary<string, int> vocab_tags;

        string _path;

        public CoNLLDataset(string path)
        {
            if (vocab_chars == null)
                vocab_chars = load_vocab("filepath_chars");

            if (vocab_words == null)
                vocab_words = load_vocab("filepath_words");

            if (vocab_tags == null)
                vocab_tags = load_vocab("filepath_tags");

            _path = path;
        }

        private (int[], int) processing_word(string word)
        {
            var char_ids = word.ToCharArray().Select(x => vocab_chars[x.ToString()]).ToArray();

            // 1. preprocess word
            if (true) // lowercase
                word = word.ToLower();
            //if (false) // isdigit
            //word = "$NUM$";

            // 2. get id of word
            //int id = vocab_words.GetValueOrDefault(word, vocab_words["$UNK$"]);
            int id = 0;
            return (char_ids, id);
        }

        private int processing_tag(string word)
        {
            // 1. preprocess word
            //if (false) // lowercase
            //word = word.ToLower();
            //if (false) // isdigit
            //word = "$NUM$";

            // 2. get id of word
            // int id = vocab_tags.GetValueOrDefault(word, -1);
            int id = 0;

            return id;
        }

        private Dictionary<string, int> load_vocab(string filename)
        {
            var dict = new Dictionary<string, int>();
            int i = 0;
            File.ReadAllLines(filename)
                .Select(x => dict[x] = i++)
                .Count();
            return dict;
        }

    }
}
