using System;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using UnityEngine;

var session = ArchipelagoSessionFactory.CreateSession("localhost", 38218);

static LoginResult TryConnectAndLogin(
    string game, 
    string slotName,  // fetch from the main menu stuff
    ItemsHandlingFlags itemsHandlingFlags,
    Version version = null,  // Minimum world version as specified in the apworld
    string[] tags = null, // tags to add to the player
    string uuid = null, // player uuid
    string password = null, // password for the multi
    bool requestSlotData = true // request slot data from the server
    )

LoginResult result = session.TryConnectAndLogin("Snailiad", "Zed", ItemsHandlingFlags.AllItems);

private static void Connect(string server, string user, string pass)
    {
        LoginResult result;

        try
        {
            // handle TryConnectAndLogin attempt here and save the returned object to `result`
        }
        catch (Exception e)
        {
            result = new LoginFailure(e.GetBaseException().Message);
        }

        if (!result.Successful)
        {
            LoginFailure failure = (LoginFailure)result;
            string errorMessage = $"Failed to Connect to {server} as {user}:";
            foreach (string error in failure.Errors)
            {
                errorMessage += $"\n    {error}";
            }
            foreach (ConnectionRefusedError error in failure.ErrorCodes)
            {
                errorMessage += $"\n    {error}";
            }

            return; // Did not connect, show the user the contents of `errorMessage`
        }
    
        // Successfully connected, `ArchipelagoSession` (assume statically defined as `session` from now on) can now be used to interact with the server and the returned `LoginSuccessful` contains some useful information about the initial connection (e.g. a copy of the slot data as `loginSuccess.SlotData`)
        var loginSuccess = (LoginSuccessful)result;