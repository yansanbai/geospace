using System;
using System.Text;

namespace TexDrawLib
{
    /// <summary>
    /// A high performance string splitter
    /// </summary>
    public class StringSplitter
    {
        /// <summary>
        /// Create a new StringSplitter object with the given buffer size
        /// </summary>
        /// <param name="bufferSize"></param>
        public StringSplitter(int bufferSize)
        {
            buffer = new string[bufferSize];
        }

        /// <summary>
        /// The string buffer container
        /// </summary>
        public string[] buffer;

        /// <summary>
        /// Get the results of the last split call
        /// </summary>
        public string[] Results { get { return buffer; } }

        public int Split(string value, char separator)
        {
            int resultIndex = 0;
            int startIndex = 0;

            // Find the mid-parts
            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] == separator)
                {
                    buffer[resultIndex] = value.Substring(startIndex, i - startIndex);
                    resultIndex++;
                    startIndex = i + 1;
                }
            }

            // Find the last part
            buffer[resultIndex] = value.Substring(startIndex, value.Length - startIndex);
            resultIndex++;

            return resultIndex;
        }

        public int SafeSplit(string value, char separator)
        {
            int resultIndex = 0;
            int startIndex = 0;
            int totalIndex = 1;

            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] == separator)
                    totalIndex++;
            }

            if (buffer.Length < totalIndex)
                Array.Resize(ref buffer, totalIndex);

            // Find the mid-parts
            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] == separator)
                {
                    // Check if the array needs to be resized

                    buffer[resultIndex++] = value.Substring(startIndex, i - startIndex);
                    startIndex = i + 1;
                }
            }

            // Find the last part
            buffer[resultIndex++] = value.Substring(startIndex, value.Length - startIndex);

            return resultIndex;
        }

        private static StringBuilder m_builder;

        public string Join(int length, string separator)
        {
            if (m_builder == null)
                m_builder = new StringBuilder();
            else
                m_builder.Length = 0;
            for (int i = 0; i < length; i++)
            {
                m_builder.Append(buffer[i]);
                if (i < length - 1)
                    m_builder.Append(separator);
            }
            return m_builder.ToString();
        }
    }
}
