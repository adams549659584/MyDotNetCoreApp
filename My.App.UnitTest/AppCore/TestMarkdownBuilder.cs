using Microsoft.VisualStudio.TestTools.UnitTesting;
using My.App.Core;
using System.Collections.Generic;
using System.Text;

namespace My.App.UnitTest
{
    [TestClass]
    public class TestMarkdownBuilder
    {
        [TestMethod]
        public void TestBuild()
        {
            MarkdownBuilder mdBuilder = new MarkdownBuilder();
            mdBuilder.AppendText("这是大标题", TextStyleType.Heading1)
                .AppendHorizontalRule();

            //代码段
            mdBuilder.AppendText("这是代码段", TextStyleType.Heading1);
            StringBuilder sbCode = new StringBuilder();
            sbCode.AppendLine("# 代码段");
            sbCode.AppendLine("git clone http://gogs.360kad.com/kad2.0/kad-mini-program-wiki.git");
            sbCode.AppendLine();
            mdBuilder.AppendCode(sbCode.ToString(), "bash");
            mdBuilder.AppendHorizontalRule();

            //无序列表
            mdBuilder.AppendText("这是无序列表", TextStyleType.Heading1)
                .AppendText("无序列表1", TextStyleType.UnorderedList)
                .AppendText("无序列表2", TextStyleType.UnorderedList)
                .AppendText("无序列表3", TextStyleType.UnorderedList)
                .AppendText("无序列表4", TextStyleType.UnorderedList)
                .AppendHorizontalRule();

            //有序列表
            mdBuilder.AppendText("这是有序列表", TextStyleType.Heading1)
                .AppendOrderedList(new string[]
                {
                "有序列表",
                "有序列表",
                "有序列表",
                "有序列表",
                })
                .AppendHorizontalRule();

            //文本风格
            mdBuilder.AppendText("这是文本风格", TextStyleType.Heading1);
            mdBuilder.AppendText(mdBuilder.FormatTextStyle("Common", TextStyleType.Common), TextStyleType.UnorderedList);
            mdBuilder.AppendText(mdBuilder.FormatTextStyle("Bold", TextStyleType.Bold), TextStyleType.UnorderedList);
            mdBuilder.AppendText(mdBuilder.FormatTextStyle("Italic", TextStyleType.Italic), TextStyleType.UnorderedList);
            mdBuilder.AppendText(mdBuilder.FormatTextStyle("Strikethrough", TextStyleType.Strikethrough), TextStyleType.UnorderedList);
            mdBuilder.AppendText(mdBuilder.FormatTextStyle("Insert", TextStyleType.Insert), TextStyleType.UnorderedList);
            mdBuilder.AppendText(mdBuilder.FormatTextStyle("Mark", TextStyleType.Mark), TextStyleType.UnorderedList);
            mdBuilder.AppendHorizontalRule();

            //表格
            mdBuilder.AppendText("这是表格", TextStyleType.Heading1);
            var mdTabel = new MarkdownTable()
            {
                Rows = new List<MarkdownTableRow>() {
                    //表格头
                    new MarkdownTableRow(){
                        Columns= new List<string>(){"header 1","header 2","header 3"}
                    },
                    //表格内容
                    new MarkdownTableRow(){
                        Columns= new List<string>(){"row 1 col 1","row 1 col 2","row 1 col 3"}
                    },
                    new MarkdownTableRow(){
                        Columns= new List<string>(){"row 2 col 1","row 2 col 2","row 2 col 3"}
                    },
                    new MarkdownTableRow(){
                        Columns= new List<string>(){"row 3 col 1","row 3 col 2","row 3 col 3"}
                    },
                }
            };
            mdBuilder.AppendTable(mdTabel);

            //源码
            string mdTest = mdBuilder.ToString();

            //保存
            string path = PathHelper.MapFile("Content\\Markdown", "test.md");
            mdBuilder.Save(path, false);
        }
    }
}
