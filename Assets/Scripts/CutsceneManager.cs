using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

[RequireComponent(typeof(BoxCollider2D))]
public class CutsceneManager : MonoBehaviour, IRoomObject {

    public TextAsset script;
    public bool active = true;

    private BoxCollider2D trig;

    public static Dictionary<string, Unit> tokens;
    [System.NonSerialized]
    public bool ready = false;

    public static readonly string myType = "Cutscene Manager";

    public string objType {
        get {
            return myType;
        }
    }

    public Dictionary<string, object> save() {
        Dictionary<string, object> content = new Dictionary<string, object>();
        content["active"] = active;
        return content;
    }

    public void load(Dictionary<string, object> content) {
        active = (bool)content["active"];
    }

    public Dictionary<string, object> resave() {
        Dictionary<string, object> content = new Dictionary<string, object>();
        content["active"] = active;
        return content;
    }

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
        trig = GetComponent<BoxCollider2D>();
        tokens = new Dictionary<string, Unit>();
        declare("true", new Unit("costBool", true));
        declare("false", new Unit("costBool", false));
        declare("True", new Unit("costBool", true));
        declare("False", new Unit("costBool", false));
        makeCond();
        declare("delay", new Unit(delay, 1));
    }

    void verfy() {
        
    }
}
