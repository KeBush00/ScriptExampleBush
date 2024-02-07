using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Logger class logs to the console with various functions and itsw primary purpose is to keep
/// the console as clean as possible.
/// </summary>
public class Logger
{
   // if script has a boolean that allows to see log then if it is true show the logs the script sends to console if not then dont show log messages
    public static void Log(string textLog, bool logStatus ) {
        if (logStatus) {
            Debug.Log(textLog);
        }
    }

    //Regardless if script has boolean or not show the message without checking for boolean in parameter
    public static void specialLog(string textLog) {
        Debug.Log(textLog);
    }
}
