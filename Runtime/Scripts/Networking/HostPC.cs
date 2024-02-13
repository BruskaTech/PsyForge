﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace UnityEPL {

    // Host PC State Message
    public partial class HostPcStateMsg {
        public readonly string name;
        public readonly Dictionary<string, object> dict; 

        private HostPcStateMsg(string name) {
            this.name = name;
            this.dict = new();
        }
        private HostPcStateMsg(string name, Dictionary<string, object> dict) {
            this.name = name;
            this.dict = dict;
        }

        public static HostPcStateMsg REST() { return new HostPcStateMsg("REST"); }
        public static HostPcStateMsg ORIENT() { return new HostPcStateMsg("ORIENT"); }
        public static HostPcStateMsg COUNTDOWN() { return new HostPcStateMsg("COUNTDOWN"); }
        public static HostPcStateMsg TRIAL() { return new HostPcStateMsg("TRIAL"); }
        public static HostPcStateMsg TRIALEND() { return new HostPcStateMsg("TRIALEND"); }
        public static HostPcStateMsg DISTRACT() { return new HostPcStateMsg("DISTRACT"); }
        public static HostPcStateMsg INSTRUCT() { return new HostPcStateMsg("INSTRUCT"); }
        public static HostPcStateMsg WAITING() { return new HostPcStateMsg("WAITING"); }
        public static HostPcStateMsg SYNC() { return new HostPcStateMsg("SYNC"); }
        public static HostPcStateMsg VOCALIZATION() { return new HostPcStateMsg("VOCALIZATION"); }
        public static HostPcStateMsg FIXATION() { return new HostPcStateMsg("FIXATION"); }
        public static HostPcStateMsg ENCODING() { return new HostPcStateMsg("ENCODING"); }
        public static HostPcStateMsg RETRIEVAL() { return new HostPcStateMsg("RETRIEVAL"); }
        public static HostPcStateMsg WORD() { return new HostPcStateMsg("WORD"); }
        public static HostPcStateMsg MATH() { return new HostPcStateMsg("MATH"); }
        public static HostPcStateMsg ISI(float duration) { return new HostPcStateMsg("ISI", new() {{"duration", duration}}); }
        public static HostPcStateMsg RECALL(float duration) { return new HostPcStateMsg("RECALL", new() {{"duration", duration}}); }
        public static HostPcStateMsg FINAL_RECALL(float duration) { return new HostPcStateMsg("FINAL_RECALL", new() {{"duration", duration}}); }
        public static HostPcStateMsg RECOGNITION(float duration) { return new HostPcStateMsg("RECOGNITION", new() {{"duration", duration}}); }
    }

    // Host PC Closed Loop Message
    public class HostPcClMsg {
        public readonly string name;
        public readonly Dictionary<string, object> dict;

        private HostPcClMsg(string name, Dictionary<string, object> dict) {
            this.name = name;
            this.dict = dict;
        }

        public static HostPcClMsg STIM(uint durationMs) { return new HostPcClMsg("STIM", new() {{ "classifyms", durationMs }}); }
        public static HostPcClMsg SHAM(uint durationMs) { return new HostPcClMsg("SHAM", new() {{ "classifyms", durationMs }}); }
        public static HostPcClMsg NORMALIZE(uint durationMs) { return new HostPcClMsg("NORMALIZE", new() {{ "classifyms", durationMs }}); }
    }

    // Host PC Continuous Closed Loop Message
    public class HostPcCclMsg {
        public readonly string name;
        public readonly Dictionary<string, object> dict;

        private HostPcCclMsg(string name) {
            this.name = name;
            this.dict = new();
        }
        private HostPcCclMsg(string name, Dictionary<string, object> dict) {
            this.name = name;
            this.dict = dict;
        }

        public static HostPcCclMsg START_STIM(int durationS) { return new HostPcCclMsg("START_STIM", new() {{"duration", durationS}}); }
        public static HostPcCclMsg PAUSE_STIM() { return new HostPcCclMsg("PAUSE_STIM"); }
        public static HostPcCclMsg RESUME_STIM() { return new HostPcCclMsg("RESUME_STIM"); }
        public static HostPcCclMsg STOP_STIM() { return new HostPcCclMsg("STOP_STIM"); }
    }

    public abstract class HostPC : NetworkInterface {
        protected abstract Task DoLatencyCheckTS();
        protected abstract CancellationTokenSource DoHeartbeatsForeverTS();

        public abstract Task ConfigureTS();
        public abstract Task QuitTS();

        public abstract Task SendMathMsgTS(string problem, string response, int responseTimeMs, bool correct);
        public abstract Task SendStimSelectMsgTS(string tag);
        public abstract Task SendStimMsgTS();
        public abstract Task SendCLMsgTS(HostPcClMsg type);
        public abstract Task SendCCLMsgTS(HostPcCclMsg type);
        public abstract Task SendSessionMsgTS(int session);
        public abstract Task SendStateMsgTS(HostPcStateMsg state, Dictionary<string, object> extraData = null);
        public abstract Task SendTrialMsgTS(int trial, bool stim);
        public abstract Task SendWordMsgTS(string word, int serialPos, bool stim, Dictionary<string, object> extraData = null);
        public abstract Task SendExitMsgTS();
        public abstract Task SendLogMsgTS(string type, Dictionary<string, object> data = null);
    }

}