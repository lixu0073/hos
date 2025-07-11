using UnityEngine;
using System.Collections;
using System;
using System.IO;

/// <summary>
/// 服务器时间控制器，用于获取和管理游戏中的服务器时间。
/// </summary>
public class ServerTime : MonoBehaviour
{
    // 服务器时间，默认为Unix纪元时间
    private DateTime serverTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    // 更新服务器时间的协程
    private Coroutine updateServerTimeCoroutine;
    // ServerTime的单例实例
    private static ServerTime time;
    // 标记服务器时间是否已下载
    public bool isTimeDownloaded = false;

    #if UNITY_EDITOR
        // 调试时显示的时间字符串
        public string debugTime;
    #endif

    // NIST服务器列表
    string[] servers = new string[]
    {
        "time-a.nist.gov",
        "time-b.nist.gov",
        "time-c.nist.gov",
        "time-d.nist.gov",
        "nist1-macon.macon.ga.us",
        "wolfnisttime.com",
        "nist.netservicesgroup.com",
        "nisttime.carsoncity.k12.mi.us",
        "nist1-lnk.binary.net",
        "wwv.nist.gov",
        "time-a.timefreq.bldrdoc.gov",
        "time-b.timefreq.bldrdoc.gov",
        "time-c.timefreq.bldrdoc.gov",
        "time.nist.gov",
        "utcnist.colorado.edu",
        "utcnist2.colorado.edu",
        "ntp-nist.ldsbc.net",
        "time-nw.nist.gov",
        "nist-time-server.eoni.com",
        "nist-time-server.eoni.com"
    };

    /// <summary>
    /// 获取ServerTime的单例实例。
    /// </summary>
    /// <returns>ServerTime实例。</returns>
    public static ServerTime Get()
    {
        return time;
    }

    /// <summary>
    /// Unity生命周期方法，在对象被加载时调用。
    /// </summary>
    void Awake()
    {
        time = this;
        //DebugServerTime();
        // GetServerTimeAndStartUpdate();
    }

    /// <summary>
    /// Unity生命周期方法，在对象禁用时调用。
    /// </summary>
    private void OnDisable()
    {
        StopAllCoroutines(); //停止此MonoBehaviour上的所有协程
    }

    /// <summary>
    /// 获取服务器时间。目前直接返回本地当前时间。
    /// </summary>
    /// <returns>服务器时间。</returns>
    public DateTime GetServerTime()
    {
        isTimeDownloaded = true;
        serverTime = DateTime.Now;
        return serverTime;
		/*if (isTimeDownloaded)
        {
            return serverTime;
        }
        else
        {

            if (!GetServerTimeAndStartUpdate())
            {
                BaseUIController.ShowCriticalProblemPopup("Can't get time from NIST servers!");
                return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            }
            else return serverTime;
        }*/
    }

    /// <summary>
    /// 获取当前服务器时间。
    /// </summary>
    /// <returns>当前服务器时间。</returns>
    public DateTime GetCurrentTime()
    {
       return serverTime;
    }

    /// <summary>
    /// 再次尝试获取服务器时间并开始更新。
    /// </summary>
    /// <returns>服务器时间。</returns>
	public DateTime GetServerTimeAgain()
	{
		if (!GetServerTimeAndStartUpdate())
		{
            BaseUIController.ShowCriticalProblemPopup(this, "Can't get time from NIST servers!");
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		}
		else return serverTime;
	}

    /// <summary>
    /// 从指定主机下载时间。
    /// </summary>
    /// <param name="host">NIST服务器主机名。</param>
    /// <param name="date">输出参数，下载到的时间。</param>
    /// <returns>如果成功下载时间，则返回true；否则返回false。</returns>
    public bool DownloadTime(string host, out DateTime date)
    {
        string serverResponse = string.Empty;

        try
        {
            StreamReader reader = new StreamReader(new System.Net.Sockets.TcpClient(host, 13).GetStream());
            serverResponse = reader.ReadToEnd();
            reader.Close();
            //reader.Dispose();

            // 检查签名是否存在
            if (serverResponse.Length > 47 && serverResponse.Substring(38, 9).Equals("UTC(NIST)"))
            {
                // 解析日期
                int jd = int.Parse(serverResponse.Substring(1, 5), System.Globalization.CultureInfo.InvariantCulture);
                int yr = int.Parse(serverResponse.Substring(7, 2), System.Globalization.CultureInfo.InvariantCulture);
                int mo = int.Parse(serverResponse.Substring(10, 2), System.Globalization.CultureInfo.InvariantCulture);
                int dy = int.Parse(serverResponse.Substring(13, 2), System.Globalization.CultureInfo.InvariantCulture);
                int hr = int.Parse(serverResponse.Substring(16, 2), System.Globalization.CultureInfo.InvariantCulture);
                int mm = int.Parse(serverResponse.Substring(19, 2), System.Globalization.CultureInfo.InvariantCulture);
                int sc = int.Parse(serverResponse.Substring(22, 2), System.Globalization.CultureInfo.InvariantCulture);

                if (jd > 51544)
                    yr += 2000;
                else
                    yr += 1999;

                Debug.Log("Time from host: " + host + " downloaded. ");
                date = new DateTime(yr, mo, dy, hr, mm, sc);
                return true;
            }

            Debug.Log("Can't get time from host: " + host + ", try get time from another server. ");
            date = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return false;
        }
        catch (Exception)
        {
            Debug.Log("Can't get time from host: " + host + ", try get time from another server. ");
            date = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return false;
        }
    }

    /// <summary>
    /// 尝试从NIST服务器获取时间并开始更新。
    /// </summary>
    /// <returns>如果成功获取并开始更新，则返回true；否则返回false。</returns>
    public bool GetServerTimeAndStartUpdate()
    {
        for (int i = 0; i < servers.Length; i++)
        {
             isTimeDownloaded = DownloadTime(servers[i], out serverTime);
             if (isTimeDownloaded)
             {
                try
                { 
                    if (updateServerTimeCoroutine != null)
                    {
                        StopCoroutine(updateServerTimeCoroutine);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
                }
                updateServerTimeCoroutine = StartCoroutine(UpdateServerTimeCoroutine());
                Debug.Log("Time downloaded from server");
                return true;
             }
        }

        isTimeDownloaded = false;
        return false;
    }

    /// <summary>
    /// 获取服务器时间，但不开始更新协程。
    /// </summary>
    /// <param name="servTime">输出参数，获取到的服务器时间。</param>
    /// <returns>如果成功获取时间，则返回true；否则返回false。</returns>
    public bool GetServerTimeWithoutUpdate(out DateTime servTime)
    {
        for (int i = 0; i < servers.Length; i++)
        {
			if (DownloadTime(servers[i], out servTime))
            {
                return true;
            }
        }

        servTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return false;
    }

    /// <summary>
    /// 将DateTime转换为Unix时间戳（秒）。
    /// </summary>
    /// <param name="dateTime">要转换的DateTime对象。</param>
    /// <returns>Unix时间戳。</returns>
    public double DateTimeToUnixTimestamp(DateTime dateTime)
    {
        return (TimeZoneInfo.ConvertTimeToUtc(dateTime) - new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).TotalSeconds;
    }

    /// <summary>
    /// 将Unix时间戳（秒）转换为DateTime。
    /// </summary>
    /// <param name="unixTimeStamp">Unix时间戳。</param>
    /// <returns>DateTime对象。</returns>
    public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
    {
        DateTime unixStart = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
        long unixTimeStampInTicks = (long)(unixTimeStamp * TimeSpan.TicksPerSecond);
        return new DateTime(unixStart.Ticks + unixTimeStampInTicks, System.DateTimeKind.Utc);
    }

    /// <summary>
    /// 获取当前服务器时间的Unix时间戳。
    /// </summary>
    /// <returns>Unix时间戳。</returns>
    public long GetUnixServerTime()
    {
        return UnixTime(GetServerTime());
    }

    /// <summary>
    /// 将Unix时间戳（秒）转换为DateTime。
    /// </summary>
    /// <param name="unixTime">Unix时间戳。</param>
    /// <returns>DateTime对象。</returns>
    public static DateTime UnixTimestampToDateTime(double unixTime)
    {
        DateTime unixStart = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
        long unixTimeStampInTicks = (long)(unixTime * TimeSpan.TicksPerSecond);
        return new DateTime(unixStart.Ticks + unixTimeStampInTicks, System.DateTimeKind.Utc);
    }

    /// <summary>
    /// 获取当前UTC时间的总秒数。
    /// </summary>
    /// <returns>总秒数。</returns>
    public static double getTime()
    {
        var timeSpan = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        return timeSpan.TotalSeconds;
    }

    /// <summary>
    /// 获取当前UTC时间的总毫秒数。
    /// </summary>
    /// <returns>总毫秒数。</returns>
    public static double getMilliSecTime()
    {
        var timeSpan = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        return timeSpan.TotalMilliseconds;
    }

    /// <summary>
    /// 将DateTime转换为Unix时间戳（秒），可选择添加偏移量。
    /// </summary>
    /// <param name="date">要转换的DateTime对象。</param>
    /// <param name="addOffset">要添加的秒数偏移量。</param>
    /// <returns>Unix时间戳。</returns>
    public static long UnixTime(DateTime date, int addOffset = 0)
    {
        var timeSpan = (date - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        return (long)timeSpan.TotalSeconds + addOffset;
    }

    /// <summary>
    /// 计算给定日期与服务器时间之间的秒数差。
    /// </summary>
    /// <param name="date">要比较的日期。</param>
    /// <returns>时间差（秒）。</returns>
    public double CalcDifferenceBetweenDateAndServerTime(DateTime date)
    {
		return (serverTime - date).TotalSeconds;
    }

    /// <summary>
    /// 停止更新服务器时间的协程。
    /// </summary>
    public void StopUpdateServerTime()
    {
        try
        { 
            if (updateServerTimeCoroutine != null)
            {
                StopCoroutine(updateServerTimeCoroutine);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
        }
        isTimeDownloaded = false;
    }

    /// <summary>
    /// 更新服务器时间的协程，每秒更新一次。
    /// </summary>
    public IEnumerator UpdateServerTimeCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            serverTime = serverTime.AddSeconds(1);

           // TRIAL TIME LIMIT CODE
           // if (serverTime.Month > 7)
           // {
           //     UIController.get.loseConnectionPopUp.Open("Your demo access has expired!", true);
           //     StopUpdateServerTime();
           //     yield return null;
           // }

            #if UNITY_EDITOR
            debugTime = serverTime.ToString() + " | " + DateTimeToUnixTimestamp(serverTime).ToString();
            #endif
        }
    }
}