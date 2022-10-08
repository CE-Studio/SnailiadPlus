using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class CutsceneManager : MonoBehaviour {

    public TextAsset script;

    public static Dictionary<string, Unit> tokens;

    public struct Unit {
        public string type;
        public object obj;
        public int argnum;

        public Unit(string t, object o) {
            type = t;
            obj = o;
            argnum = -1;
        }

        public Unit(System.Func<object[], bool> o, int c) {
            type = "func";
            obj = o;
            argnum = c;
        }
    }

    public static void declare(string name, Unit data) {
        if (tokens.ContainsKey(name)) {
            throw new System.Data.DuplicateNameException("Name '" + name + "' already exists.");
        }
        tokens[name] = data;
    }

    public static bool delay(object[] args) {
        float h = (float)args[0];
        Thread.Sleep((int)(h * 1000));
        return true;
    }

    private static void makeCond() {
        Dictionary<string, Unit> hold = new Dictionary<string, Unit> {
            {"always", new Unit("constInt", 0)},
            {"onGround", new Unit("constInt", 0)},
        };
        declare("CONDITIONS", new Unit("dict", hold));
    }

    void Awake() {
        tokens = new Dictionary<string, Unit>();
        declare("true", new Unit("costBool", true));
        declare("false", new Unit("costBool", false));
        declare("True", new Unit("costBool", true));
        declare("False", new Unit("costBool", false));
        makeCond();
    }

    void Update() {
        
    }
}
