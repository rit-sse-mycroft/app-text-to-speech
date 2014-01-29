#Text to Speech

## Interface

This app's capability is `tts`. Be sure to list it in your dependencies. To speak text send a message of the following format:

```json
MSG_QUERY {
  "id" : "339f368c-11a5-496f-aeef-43402b60959c",
  "capability" : "tts",
  "instanceId" : [ "thisInstanceId" ],
  "data" : { 
     "text" : [{
         "phrase": "text to speak",
         "delay": xx,
     }],
     "targetSpeaker" : "targetSpeakerInstanceId" 
  },
  "priority" : 30,
  "action" : "say"
}
```
* `id`: a newly generated GUID.
* `data`: `targetSpeaker` parameter is unused if `action` is set to `say`. If `action` is `stream` then this is the `instanceId` of the target speaker.
* `priority`: unused by this app, but is passed along to the speaker.
* `action`: either `say` or `stream`. The former reads the text out on this app's speakers, the latter streams it to the target speaker.

### Streaming Process

1. An App, for instance Trash30, sends a `MSG_QUERY` verb to the Mycroft server, which is relayed to this application.
2. This application processes the request and synthesizes speech to a wave stream stored in memory
3. This application sends the following message to the target speaker:
```json
MSG_QUERY {
  "id" : "7f9eadd0-654e-4b34-8329-573b577a09a0",
  "capability" : "speakers",
  "instanceId" : [ "targetSpeakerInstanceId" ],
  "data" : {
    "ip" : "xxx.xxx.xxx.xxx",
    "port" 32761,
    "streamType" : "wav"
  },
  "priority" : 30,
  "action" : "stream_tts"
}
```
4. The speakers connect to this application on a raw TCP connection.
5. The speakers send the id (you're just sending 36 characters)
6. The server streams the wave data over the TCP connection, which the speakers say.

## Programming Guide

The speech processing logic and speech streaming server exist in separate threads. They have a common
ConcurrentDictionary<string, MsgQuery> between them. Once the speech is processed it is stored into
the dictionary with the key being the newly generated guid and the value containing information about
the request, including the wave stream.

[.NET Speech Synthesis Documentation](http://msdn.microsoft.com/en-us/library/hh361625%28v=office.14%29.aspx)
