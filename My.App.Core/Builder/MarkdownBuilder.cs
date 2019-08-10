using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace My.App.Core
{
    /// <summary>
    /// Markdown构建
    /// </summary>
    public class MarkdownBuilder
    {
        /// <summary>
        /// Markdown文本
        /// </summary>
        private StringBuilder MarkdownText { get; set; }

        public MarkdownBuilder()
        {
            this.MarkdownText = new StringBuilder();
        }

        /// <summary>
        /// 在此实例追加指定的文本
        /// </summary>
        /// <param name="text">文本内容</param>
        /// <param name="textStyleType">文本风格类型</param>
        /// <returns></returns>
        public MarkdownBuilder AppendText(string text, TextStyleType textStyleType = TextStyleType.Common)
        {
            string formatVal = FormatTextStyle(text, textStyleType);
            this.AppendLine(formatVal);
            switch (textStyleType)
            {
                case TextStyleType.Common:
                    break;
                case TextStyleType.Heading1:
                    break;
                case TextStyleType.Heading2:
                    break;
                case TextStyleType.Heading3:
                    break;
                case TextStyleType.Heading4:
                    break;
                case TextStyleType.Heading5:
                    break;
                case TextStyleType.Heading6:
                    break;
                case TextStyleType.Bold:
                    break;
                case TextStyleType.Italic:
                    break;
                case TextStyleType.Strikethrough:
                    break;
                case TextStyleType.Insert:
                    break;
                case TextStyleType.Mark:
                    break;
                case TextStyleType.Quote:
                    this.AppendLine();
                    break;
                case TextStyleType.UnorderedList:
                    break;
                case TextStyleType.IncompleteTaskList:
                    break;
                case TextStyleType.CompleteTaskList:
                    break;
                default:
                    break;
            }
            return this;
        }

        /// <summary>
        /// 将默认的行终止符追加到当前实例的末尾。
        /// </summary>
        /// <returns></returns>
        public MarkdownBuilder AppendLine()
        {
            this.MarkdownText.AppendLine();
            return this;
        }

        /// <summary>
        /// 将后面跟有默认行终止符的指定字符串的副本追加到当前实例的末尾。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private MarkdownBuilder AppendLine(string value)
        {
            this.MarkdownText.AppendLine(value);
            return this;
        }

        /// <summary>
        /// 在此实例追加指定的html
        /// </summary>
        /// <param name="html">html</param>
        /// <returns></returns>
        public MarkdownBuilder AppendHtml(string html)
        {
            this.AppendLine("<html>");
            this.AppendLine(html);
            this.AppendLine("</html>");
            return this;
        }

        /// <summary>
        /// 在此实例追加指定的代码段
        /// </summary>
        /// <param name="code">代码</param>
        /// <param name="codeStyle">代码风格</param>
        /// <returns></returns>
        public MarkdownBuilder AppendCode(string code, string codeStyle)
        {
            this.AppendLine(string.Format("```{0}", codeStyle));
            this.AppendLine(code);
            this.AppendLine("```");
            return this;
        }

        /// <summary>
        /// 在此实例追加指定的链接
        /// </summary>
        /// <param name="linkText"></param>
        /// <param name="linkTo"></param>
        /// <returns></returns>
        public MarkdownBuilder AppendLink(string linkText, string linkTo)
        {
            this.AppendLine(string.Format("[{0}]({1})", linkText, linkTo));
            return this;
        }

        /// <summary>
        /// 在此实例追加指定的图片
        /// </summary>
        /// <param name="src">图片路径</param>
        /// <param name="alt">当图片显示不出来的时候 就会显示相应的文本</param>
        /// <returns></returns>
        public MarkdownBuilder AppendImage(string src, string alt)
        {
            this.AppendLine(string.Format("![{0}]({1})", alt, src));
            return this;
        }

        /// <summary>
        /// 在此实例追加指定的表格
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public MarkdownBuilder AppendTable(MarkdownTable table)
        {
            if (table != null && table.Rows != null && table.Rows.Count > 0)
            {
                var tbHeadRow = table.Rows[0];

                //表格头
                this.AppendTableRow(tbHeadRow);

                //分割线
                this.MarkdownText.Append("|");
                foreach (var tbColumn in tbHeadRow.Columns)
                {
                    this.MarkdownText.Append("---|");
                }
                this.MarkdownText.AppendLine();

                //表格行
                for (int i = 1; i < table.Rows.Count; i++)
                {
                    var tbRow = table.Rows[i];
                    this.AppendTableRow(tbRow);
                }
                this.AppendLine();
            }
            return this;
        }

        /// <summary>
        /// 添加表格行
        /// </summary>
        /// <param name="tbRow">表格行</param>
        private void AppendTableRow(MarkdownTableRow tbRow)
        {
            this.MarkdownText.Append("|");
            foreach (var tbColumn in tbRow.Columns)
            {
                this.MarkdownText.AppendFormat(" {0} |", tbColumn);
            }
            this.MarkdownText.AppendLine();
        }

        /// <summary>
        /// 在此实例追加水平线
        /// </summary>
        /// <returns></returns>
        public MarkdownBuilder AppendHorizontalRule()
        {
            this.MarkdownText.AppendLine();
            this.MarkdownText.AppendLine("---");
            this.MarkdownText.AppendLine();
            return this;
        }

        /// <summary>
        /// 在此实例追加有序列表
        /// </summary>
        /// <param name="texts">列表内容</param>
        /// <returns></returns>
        public MarkdownBuilder AppendOrderedList(string[] texts)
        {
            for (int i = 0; i < texts.Length; i++)
            {
                this.MarkdownText.AppendFormat("{0}. {1}", i + 1, texts[0]);
                this.MarkdownText.AppendLine();
            }
            return this;
        }

        /// <summary>
        /// 根据不同文本风格格式化字符串
        /// </summary>
        /// <param name="value"></param>
        /// <param name="textStyleType"></param>
        /// <returns></returns>
        public string FormatTextStyle(string value, TextStyleType textStyleType)
        {
            switch (textStyleType)
            {
                case TextStyleType.Common:
                    return value;
                case TextStyleType.Heading1:
                    return string.Format("# {0}", value);
                case TextStyleType.Heading2:
                    return string.Format("## {0}", value);
                case TextStyleType.Heading3:
                    return string.Format("### {0}", value);
                case TextStyleType.Heading4:
                    return string.Format("#### {0}", value);
                case TextStyleType.Heading5:
                    return string.Format("##### {0}", value);
                case TextStyleType.Heading6:
                    return string.Format("###### {0}", value);
                case TextStyleType.Bold:
                    return string.Format("**{0}**", value);
                case TextStyleType.Italic:
                    return string.Format("*{0}*", value);
                case TextStyleType.Strikethrough:
                    return string.Format("~~{0}~~", value);
                case TextStyleType.Insert:
                    return string.Format("++{0}++", value);
                case TextStyleType.Mark:
                    return string.Format("=={0}==", value);
                case TextStyleType.Quote:
                    return string.Format("> {0}", value);
                case TextStyleType.UnorderedList:
                    return string.Format("- {0}", value);
                case TextStyleType.IncompleteTaskList:
                    return string.Format("- [ ] {0}", value);
                case TextStyleType.CompleteTaskList:
                    return string.Format("- [x] {0}", value);
                default:
                    return value;
            }
        }

        /// <summary>
        /// 将此实例的值转换为 System.String。
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.MarkdownText.ToString();
        }

        /// <summary>
        /// 将此实例保存为 Markdown 文件
        /// </summary>
        /// <param name="path">要写入的完整文件路径。</param>
        /// <param name="append">若要追加数据到该文件中，则为 true；若要覆盖该文件，则为 false。</param>
        public void Save(string path, bool append)
        {
            string directoryName = Path.GetDirectoryName(path);
            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }
            using (StreamWriter sw = new StreamWriter(path, append, Encoding.UTF8))
            {
                string text = this.ToString();
                sw.Write(text);
            }
        }
    }


    /// <summary>
    /// 文本风格类型
    /// </summary>
    public enum TextStyleType
    {
        /// <summary>
        /// 普通文本
        /// </summary>
        Common,

        /// <summary>
        /// 一级标题
        /// </summary>
        Heading1,

        /// <summary>
        /// 二级标题
        /// </summary>
        Heading2,

        /// <summary>
        /// 三级标题
        /// </summary>
        Heading3,

        /// <summary>
        /// 四级标题
        /// </summary>
        Heading4,

        /// <summary>
        /// 五级标题
        /// </summary>
        Heading5,

        /// <summary>
        /// 六级标题
        /// </summary>
        Heading6,

        /// <summary>
        /// 粗体
        /// </summary>
        Bold,

        /// <summary>
        /// 斜体
        /// </summary>
        Italic,

        /// <summary>
        /// 删除线
        /// </summary>
        Strikethrough,

        /// <summary>
        /// 下划线
        /// </summary>
        Insert,

        /// <summary>
        /// 标志
        /// </summary>
        Mark,

        /// <summary>
        /// 引用
        /// </summary>
        Quote,

        /// <summary>
        /// 无序列表
        /// </summary>
        UnorderedList,

        /// <summary>
        /// 任务列表--未完成
        /// </summary>
        IncompleteTaskList,

        /// <summary>
        /// 任务列表--已完成
        /// </summary>
        CompleteTaskList,
    }

    /// <summary>
    /// Markdown 表格
    /// </summary>
    public class MarkdownTable
    {
        public MarkdownTable()
        {
            this.Rows = new List<MarkdownTableRow>();
        }
        /// <summary>
        /// Markdown 表格行 集合 ，第一行会作为表格头
        /// </summary>
        public IList<MarkdownTableRow> Rows { get; set; }
    }

    /// <summary>
    /// Markdown 表格行
    /// </summary>
    public class MarkdownTableRow
    {
        public MarkdownTableRow()
        {
            this.Columns = new List<string>();
        }
        public IList<string> Columns { get; set; }
    }
}
