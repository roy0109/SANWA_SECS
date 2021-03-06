﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;

namespace SanwaSecsDll
{
    public partial class SanwaBaseExec
    {
        public void ReplyS5F6(PrimaryMessageWrapper e, SecsMessage receiveMsg, SecsMessage replyMsg)
        {
            //儲存所有要顯示的EC
            //List<string> alarmidlist = new List<string>();
            Dictionary<string, SecsFormat> alarmidlist = new Dictionary<string, SecsFormat>();
            CheckReceiveIDList(receiveMsg, ref alarmidlist, _alarmIDList);

            string newReplyMsg = GetMessageName(replyMsg.ToSml());

            newReplyMsg += "< L[" + alarmidlist.Count.ToString() + "]\r\n";

            newReplyMsg = GetAlarmDetailItem(_alarmIDList, alarmidlist, newReplyMsg);

            newReplyMsg += ">";

            e.ReplyAsync(newReplyMsg.ToSecsMessage());
        }

        private string GetAlarmDetailItem(Dictionary<string, SanwaAlarm> alarmList, Dictionary<string, SecsFormat> alarmIDList, string ReplyMSG)
        {
            foreach (var AlarmIDObj in alarmIDList)
            {
                //解碼 ALCD、ALID、ALTX
                _smlManager._messageList.TryGetValue("S5F6", out SanwaSML smlObj);

                List<string> requestList = new List<string>();
                using (StringReader reader = new StringReader(smlObj.Text))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.Contains("ALCD"))
                        {
                            requestList.Add("ALCD");
                        }
                        else if (line.Contains("ALID"))
                        {
                            requestList.Add("ALID");
                        }
                        else if (line.Contains("ALTX"))
                        {
                            requestList.Add("ALTX");
                        }
                    }
                }


                alarmList.TryGetValue(AlarmIDObj.Key, out SanwaAlarm Obj);

                ReplyMSG += "<L["+ requestList.Count + "]\r\n";
                //ReplyMSG += "<A[0]" + iD + ">\r\n";

                if (Obj == null)
                {
                    ReplyMSG += "<B[0]>\r\n";
                    //ReplyMSG += "<U4[0]"+ Obj._id + ">\r\n";
                    ReplyMSG += GetToSMLItem(AlarmIDObj.Value, AlarmIDObj.Key);
                    ReplyMSG += "<A[0]>\r\n";
                    ReplyMSG += ">\r\n";
                    continue;
                }

                if (Obj._set)
                {
                    ReplyMSG += "<B[0]0x80>\r\n";
                }
                else
                {
                    ReplyMSG += "<B[0]0x00>\r\n";
                }

                //ReplyMSG += "<U4[0]" + Obj._id + ">\r\n";
                ReplyMSG += GetToSMLItem(AlarmIDObj.Value, Obj._id);
                ReplyMSG += "<A[0]"+ Obj._text + ">\r\n";
                ReplyMSG += ">\r\n";
            }

            return ReplyMSG;
        }
    }
}
