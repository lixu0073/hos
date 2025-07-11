using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

/// <summary>
/// 脏词检查器，用于从文本资源中加载脏词列表，并提供替换字符串中脏词的功能。
/// </summary>
public class BadWordChecker : MonoBehaviour
{
    // 从左到右书写语言的脏词文本资源数组
    public TextAsset[] sourcesLTR;
    // 从右到左书写语言的脏词文本资源数组
    public TextAsset[] sourcesRTL;

    // 存储所有脏词的正则表达式列表
    public static List<Regex> badWordsRegexs;

    // Unity生命周期方法，在对象创建时调用
    void Awake()
    {
        // 防止在加载新场景时销毁此对象
        DontDestroyOnLoad(gameObject);
        badWordsRegexs = new List<Regex>();

        // 处理从左到右的语言
        foreach ( var source in sourcesLTR)
        {
            StringReader sr = new StringReader(source.text);
            string line = "";
            int index = -1;
            while(sr.Peek() != -1)
            {
                line = sr.ReadLine();
                // 忽略注释
                index = line.IndexOf('#');
                if(index != -1)
                {
                    line = line.Substring(0, index);
                }
                if (!string.IsNullOrEmpty(line))
                {
                    line = line.Replace("\\b", "");
                    badWordsRegexs.Add(new Regex(line, RegexOptions.IgnoreCase));
                }
            }
            sr.Close();
        }

        // 处理从右到左的语言
        foreach (var source in sourcesRTL)
        {
            StringReader sr = new StringReader(source.text);
            string line = "";
            int index = -1;
            while (sr.Peek() != -1)
            {
                line = sr.ReadLine();
                // 忽略注释
                index = line.IndexOf('#');
                if (index != -1)
                {
                    line = line.Substring(0, index);
                }
                if (!string.IsNullOrEmpty(line))
                {
                    line = line.Replace("\\b", "");
                    badWordsRegexs.Add(new Regex(line, RegexOptions.IgnoreCase | RegexOptions.RightToLeft));
                }
            }
            sr.Close();
        }
    }
}

/// <summary>
/// 提供字符串扩展方法。
/// </summary>
static class StringExtention
{
    /// <summary>
    /// 将字符串中的脏词替换为星号(*)。
    /// </summary>
    /// <param name="text">要处理的文本</param>
    /// <returns>处理后的文本</returns>
    public static string ReplaceBadWords(this string text)
    {
        if (BadWordChecker.badWordsRegexs == null) return text;

        string result = text;
        foreach(var regex in BadWordChecker.badWordsRegexs)
        {
            MatchCollection matches = regex.Matches(text);

            foreach (Match match in matches)
            {
                foreach (Capture capture in match.Captures)
                {
                    result = result.Replace(capture.Value, new string('*',capture.Value.Length));
                }
            }
        }

        return result;
    }
}