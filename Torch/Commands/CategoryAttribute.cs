using System;
using System.Collections.Generic;
using System.Linq;

namespace Torch.Commands
{
    public class CategoryAttribute : Attribute
    {
        /// <summary>
        ///     Provides information about where to place commands in the command tree. Supports space-delimited hierarchy.
        /// </summary>
        /// <param name="category"></param>
        public CategoryAttribute(string category)
        {
            Path = category.Split(' ').ToList();
        }

        public List<string> Path { get; }
    }
}