using Microsoft.Toolkit.Parsers.Markdown.Blocks;
using System;
using System.Collections.Generic;
using System.Text;

namespace CodeScripter
{
    public class MarkdownCursor
    {
        private readonly IList<MarkdownBlock> blocks;
        private int currentIdx;

        public MarkdownCursor(IList<MarkdownBlock> blocks)
        {
            this.blocks = blocks;
            currentIdx = 0;
        }

        public MarkdownBlock Current {
            get
            {
                if (currentIdx >= 0 && currentIdx < blocks.Count) return blocks[currentIdx];

                return null;
            }
        }

        public bool MoveNext()
        {
            if (currentIdx < blocks.Count) currentIdx++;

            return Current != null;
        }

        public bool MovePrevious()
        {
            if (currentIdx >= 0) currentIdx--;

            return Current != null;
        }

        public void Reset()
        {
            currentIdx = 0;
        }

        public void End()
        {
            currentIdx = blocks.Count;
        }
    }
}
