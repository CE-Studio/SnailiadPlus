using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRoomObject {
    public string objType { get; }
    public Dictionary<string, object> save();
    public void load(Dictionary<string, object> content);
}

/*
public static readonly string myType = "Fake Boundary";

public string objType {
    get {
        return myType;
    }
}

public Dictionary<string, object> save() {
    Dictionary<string, object> content = new Dictionary<string, object>();
    content[""] = ;
}

public void load(Dictionary<string, object> content) {

}
*/