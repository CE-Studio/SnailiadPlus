using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICutsceneObject {
    public void cutRegister(); //declare properties

    public void cutStart(); //begin cutscene

    public void cutEnd(); //end cutscene
}
