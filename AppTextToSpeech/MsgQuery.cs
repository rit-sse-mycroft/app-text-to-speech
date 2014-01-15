using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json.Linq;

namespace AppTextToSpeech
{

  class MsgQuery
  {
    public string TargetSpeakers;
    public string Text;
    public string OriginalUUID;
    public string NewUUID;
    public string Action;
    public int Priority;
    public Stream Output;

    /// <summary>
    /// Construct a MsgQuery from a json object
    /// </summary>
    /// <param name="json">the json used</param>
    public MsgQuery(dynamic json)
    {
      this.OriginalUUID = json.id;
      this.Action = json.action;
      JObject data = json.data;
      JToken targetSpeakers = null;
      if (data.TryGetValue("targetSpeaker", out targetSpeakers))
        this.TargetSpeakers = targetSpeakers.ToString();
      JToken text = null;
      if (data.TryGetValue("text", out text))
        this.Text = text.ToString();
      else
        this.Text = "";
      JObject jo = json;
      JToken priority = null;
      bool found = jo.TryGetValue("priority", out priority);
      if (found)
        this.Priority = priority.Value<int>();
      else
        this.Priority = 3;
    }

    /// <summary>
    /// Create a new MsgQuery object with everything null
    /// </summary>
    public MsgQuery()
    {
    }

    public virtual int GetHashCode()
    {
      return OriginalUUID != null ? OriginalUUID.GetHashCode() : 1;
    }
  }
}