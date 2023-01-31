using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

[RequireComponent(typeof(BoxCollider2D))]
public class CutsceneManager:MonoBehaviour, IRoomObject {

    public class cutsceneCompileError:System.Exception {
        public cutsceneCompileError() { }

        public cutsceneCompileError(string content) : base("Cutscene compiler: " + content) {

        }
    }

    public class cutsceneRuntimeError:System.Exception {
        public cutsceneRuntimeError() { }

        public cutsceneRuntimeError(string content) : base("Cutscene runtime: " + content) {

        }
    }

    public TextAsset script;
    public bool active = true;

    private BoxCollider2D trig;

    public static Dictionary<string, Unit> tokens;
    [System.NonSerialized]
    public bool ready = false;
    [System.NonSerialized]
    public List<sline> lines;
    [System.NonSerialized]
    public string[] rawlines;

    public static readonly string myType = "Cutscene Manager";

    public string objType {
        get {
            return myType;
        }
    }

    public struct sline {
        public string line;
        public List<sline> indent;
        public int indentlvl;
    }

    public Dictionary<string, object> save() {
        Dictionary<string, object> content = new Dictionary<string, object>();
        content["script"] = script;
        content["active"] = active;
        return content;
    }

    public void load(Dictionary<string, object> content) {
        script = (TextAsset)content["script"];
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
        public string returntype;

        public Unit(string t, object o) {
            type = t;
            returntype = t;
            obj = o;
            argnum = -1;
        }

        public Unit(System.Func<object[], object[]> o, int c, string rt) {
            type = "func";
            obj = o;
            argnum = c;
            returntype = rt;
        }
    }

    public static void declare(string name, Unit data) {
        if (tokens.ContainsKey(name)) {
            throw new System.Data.DuplicateNameException("Name '" + name + "' already exists.");
        }
        tokens[name] = data;
    }

    public static object[] delay(object[] args) {
        float h = (float)args[0];
        Thread.Sleep((int)(h * 1000));
        return new object[1] {true};
    }

    private static void makeCond() {
        Dictionary<string, Unit> hold = new Dictionary<string, Unit> {
            {"always", new Unit("constInt", 0)},
            {"onGround", new Unit("constInt", 1)},
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
        declare("delay", new Unit(delay, 1, "none"));
    }

    void Start() {
        verfy();
        tokenize(lines);
    }

    void verfy() {
        try {
            rawlines = script.text.Split(new string[] { "\r\n", "\r", "\n" }, System.StringSplitOptions.RemoveEmptyEntries);
            lines = extract(0, 0, out _);
        } catch (System.Exception e) {
            string em = "//Error!!\n\n" +
                e.Message + '\n' +
                "In file \"" + script.name + "\"\n\n" +
                e.StackTrace + "\n\n" +
                "Cutscene aborted\nPress <Esc> to continue...\n";
            print(em);
            errorcontrol.text = em;
            Instantiate(Resources.Load<GameObject>("error/errorscreen"));
        }
    }

    List<sline> extract(int lnum, int depth, out int nlnum) {
        List<sline> extlines = new List<sline>();
        while (lnum < (rawlines.Length - 1)) {
            int lineDepth = 0;
            int parenDepth = 0;
            bool countWhitesapce = true;
            bool skipNext = false;
            bool quote = false;
            bool lineOver = false;
            List<sline> content = null;
            print(rawlines[lnum]);
            foreach (char i in rawlines[lnum]) {
                if (skipNext || lineOver) {
                    skipNext = false;
                } else {
                    if (i == '\\') {
                        skipNext = true;
                    } else {
                        switch (i) {
                            case '\t':
                            case ' ':
                                if (countWhitesapce) {
                                    lineDepth += 1;
                                }
                                break;
                            case '(':
                                if (!quote) parenDepth += 1;
                                countWhitesapce = false;
                                break;
                            case ')':
                                if (!quote) parenDepth -= 1;
                                countWhitesapce = false;
                                break;
                            case '"':
                                quote = !quote;
                                countWhitesapce = false;
                                break;
                            case '#':
                                lineOver = true;
                                break;
                            default:
                                countWhitesapce = false;
                                break;
                        }
                    }
                }
            }
            if (quote) {
                throw new cutsceneCompileError("Open string on line " + lnum);
            }
            if (parenDepth != 0) {
                throw new cutsceneCompileError("Imbalanced parenthesis on line " + lnum);
            }
            if (lineDepth > depth) {
                print("Hold my line, I'm going in!");
                content = extract(lnum, lineDepth, out lnum);
            }
            if (lineDepth < depth) {
                lnum -= 1;
                break;
            } else {
                sline a = new sline();
                a.line = rawlines[lnum].Trim();
                if (content != null) {
                    if (extlines.Count < 1) {
                        throw new cutsceneCompileError("Script cannot start with indented block");
                    }
                    sline h = extlines[extlines.Count - 1];
                    h.indent = content;
                    extlines[extlines.Count - 1] = h;
                }
                a.indentlvl = depth;
                extlines.Add(a);
            }
            lnum += 1;
        }
        print("Recursion level ended");
        nlnum = lnum;
        return extlines;
    }

    void tokenize(List<sline> slines) {
        foreach (sline thisline in slines) {
            print(thisline.line);
            if (thisline.indent != null) {
                tokenize(thisline.indent);
            }
        }
    }
}
