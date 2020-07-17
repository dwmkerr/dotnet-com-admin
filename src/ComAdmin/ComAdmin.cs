using System;
using ComAdmin.ExamineFile;

namespace ComAdmin
{
    /// <summary>
    /// The ComAdmin class contains the main APIs which are provided by this library to support COM administration.
    /// </summary>
    public class ComAdmin
    {
        /// <summary>
        /// Examine a given file to determine whether it is an assembly or native dll, and whether
        /// it contains shell extensions.
        /// </summary>
        /// <param name="filePath">The path to the file to examine.</param>
        /// <returns>The details of the file to examine.</returns>
        public static ExamineFileResult ExamineFile(string filePath)
        {
            return ExamineFileApi.ExamineFile(filePath);
        }
    }
}
