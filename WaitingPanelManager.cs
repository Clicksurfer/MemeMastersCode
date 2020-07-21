using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;

public class WaitingPanelManager : NetworkBehaviour {

    public int MinPlayerAmount = 3;
    public GameObject WaitingPanel;
    private GameObject[] playerObjects;
    public bool WaitingForPlayers = false;
    public int TimeToGameStart = 10;
    public Text MainText;
    public TextMeshProUGUI CounterText;
    public bool InGame = false;

    private float timerForGameStart = 0;
    private bool enoughPlayers = false;
    private float enoughPlayersTimer = 0f;
    private float enoughPlayersTimerLimit = 1f;
    private int numberOfPlayersPrev = 1;

    private AudioClip currentTick;
    private bool calledDoGameStartTimerThisTurn = false;
    private bool closedUpTimer = true;
    
    //Script is responsible for managing the waiting panel for all players. It will begin the game when enough players have joiend
    //It is likewise responsible for stopping the game if enough players disconnect

	void Start () 
    {
        timerForGameStart = TimeToGameStart;
	}
	
	//Update is used to check for players and activate logic accordingly
	void Update () {
        calledDoGameStartTimerThisTurn = false;
        if (isServer)
            CheckForPlayers();
        else
        {
            if (enoughPlayers)
                EnoughPlayers();
            else
                NotEnoughPlayers();
        }
	}

    [ClientRpc]
    private void RpcCatchUpNewPlayer(float timerNewVal, bool enoughPlayersNewVal, bool waitingForPlayersNewVal, bool closedUpTimerNewVal, bool inGameNewVal)
    {
        CatchUpNewPlayer(timerNewVal, enoughPlayersNewVal, waitingForPlayersNewVal, closedUpTimerNewVal, inGameNewVal);
    }

    //Function used to catch up new player who just joined to current status
    private void CatchUpNewPlayer(float timerNewVal, bool enoughPlayersNewVal, bool waitingForPlayersNewVal, bool closedUpTimerNewVal, bool inGameNewVal)
    {
        timerForGameStart = timerNewVal;
        enoughPlayers = enoughPlayersNewVal;
        WaitingForPlayers = waitingForPlayersNewVal;
        closedUpTimer = closedUpTimerNewVal;
        InGame = inGameNewVal;
    }

    //Function checks for players, and then decides if match can start or not
    private void CheckForPlayers()
    {
        playerObjects = GameObject.FindGameObjectsWithTag("Player");
        float timeBeforeChange = timerForGameStart;
        enoughPlayers = playerObjects.Length >= MinPlayerAmount;

        if (numberOfPlayersPrev != playerObjects.Length)
        {
            numberOfPlayersPrev = playerObjects.Length;
            RpcCatchUpNewPlayer(timerForGameStart, enoughPlayers, WaitingForPlayers, closedUpTimer, InGame);
        }

        if (enoughPlayers)
        {// THIS IS WHERE YOU CAN PLAY THE GAME
            EnoughPlayers();
            if ((int)timeBeforeChange != (int)timerForGameStart)
                RpcUpdateDoingTimer(timerForGameStart);
        }
        else
        {// THIS IS WHERE WE WAIT FOR PLAYERS
            NotEnoughPlayers();
            if ((int)timeBeforeChange != (int)timerForGameStart)
                RpcUpdateDoingTimer(timerForGameStart);
        }

        enoughPlayersTimer += Time.deltaTime;
        if (enoughPlayersTimer >= enoughPlayersTimerLimit)
        {
            enoughPlayersTimer = 0f;
            RpcUpdateEnoughPlayers(enoughPlayers);
        }
    }

    [ClientRpc]
    private void RpcEnoughPlayers()
    {
        EnoughPlayers();
    }

    [ClientRpc]
    private void RpcNotEnoughPlayers()
    {
        NotEnoughPlayers();
    }

    //Function is called when there are enough players
    private void EnoughPlayers()
    {
        if (WaitingForPlayers && InGame == false)
        {
            WaitingForPlayers = false;
            DoGameStartTimer();
        }
        else
        {
            if ((timerForGameStart > 0f || (timerForGameStart == 0f && closedUpTimer == false)) && InGame == false)
            {
                DoGameStartTimer();//Continue the countdown for the game to start
            }
            else if (WaitingPanel.GetComponent<RectTransform>().offsetMin.x == WaitingPanel.GetComponent<RectTransform>().offsetMax.x && WaitingPanel.GetComponent<RectTransform>().offsetMax.x == -1080)
            {
                WaitingPanel.SetActive(false);//One the match started and the waiting panel is offscreen, deactivate it.
            }
            else
            {
                //Time to begin the match!
                InGame = true;
                WaitingForPlayers = false;
                WaitingPanel.GetComponent<Animator>().SetBool("WindowOpen", false);
                WaitingPanel.GetComponent<Animator>().SetBool("Idle", false);
                this.GetComponent<TurnManager>().TurnPaused = false;
            }
        }
    }

    [ClientRpc]
    private void RpcUpdateDoingTimer(float newVal)
    {
        UpdateDoingTimer(newVal);
    }

    private void UpdateDoingTimer(float newVal)
    {
        timerForGameStart = newVal;
    }

    [ClientRpc]
    private void RpcUpdateEnoughPlayers(bool newVal)
    {
        UpdateEnoughPlayers(newVal);
    }

    private void UpdateEnoughPlayers(bool newVal)
    {
        enoughPlayers = newVal;
    }

    private void NotEnoughPlayers()
    {
        if (WaitingForPlayers)//If we were already waiting for players, continue with that
        {
            if (WaitingPanel.GetComponent<Animator>().GetBool("Idle") == false && WaitingPanel.GetComponent<RectTransform>().offsetMin.x == WaitingPanel.GetComponent<RectTransform>().offsetMax.x && WaitingPanel.GetComponent<RectTransform>().offsetMax.x == 0)
            {
                WaitingPanel.GetComponent<Animator>().SetBool("Idle", true);
            }
            else
            {
                this.GetComponent<TurnManager>().TurnPaused = true;
                WaitingForPlayers = true;
                WaitingPanel.GetComponent<Animator>().SetBool("WindowOpen", true);
                WaitingPanel.SetActive(true);
            }
        }
        else //If we are mid-match and suddenly there aren't enough players, return to waiting panel
        {
            InGame = false;
            EndGameStartTimer();
            this.GetComponent<TurnManager>().TurnPaused = true;
            WaitingForPlayers = true;
            WaitingPanel.GetComponent<Animator>().SetBool("WindowOpen", true);
            WaitingPanel.SetActive(true);
        }
        timerForGameStart = 0f;
        MainText.text = "Waiting for more players...";
    }

    //Start/progress/end the start timer
    private void DoGameStartTimer()
    {
        if (calledDoGameStartTimerThisTurn == false)
        {
            if (CounterText.gameObject.activeSelf == false)
            {
                //Setup for timer animation
                //Debug.Log("Setting up for game start countdown timer!");
                timerForGameStart = TimeToGameStart;
                CounterText.gameObject.SetActive(true);
                CounterText.gameObject.GetComponent<Animator>().SetBool("DoAnimation", true);

                CounterText.GetComponent<AudioSource>().PlayOneShot(currentTick);
                if (currentTick == this.GetComponent<TurnManager>().Tock)
                    currentTick = this.GetComponent<TurnManager>().Tick;
                else
                    currentTick = this.GetComponent<TurnManager>().Tock;
                closedUpTimer = false;
            }
            else if (timerForGameStart == 0f)
            {
                //Reached end of timer, time to return to match

                CloseUpTimer();
            }
            else
            {
                //In the middle of doing timer animation

                string newVal = (Mathf.FloorToInt(timerForGameStart) + 1).ToString();
                CounterText.gameObject.GetComponent<Animator>().SetBool("DoAnimation", (CounterText.text == newVal));
                if (CounterText.text != newVal)
                {
                    CounterText.GetComponent<AudioSource>().PlayOneShot(currentTick);
                    if (currentTick == this.GetComponent<TurnManager>().Tock)
                        currentTick = this.GetComponent<TurnManager>().Tick;
                    else
                        currentTick = this.GetComponent<TurnManager>().Tock;
                    closedUpTimer = false;
                }
                CounterText.text = newVal;
                closedUpTimer = false;
            }
            timerForGameStart = Mathf.Clamp(timerForGameStart - Time.deltaTime, 0f, TimeToGameStart);
            MainText.text = "Game starts in";
            calledDoGameStartTimerThisTurn = true;
        }
    }


    private void CloseUpTimer()
    {
        EndGameStartTimer();

        WaitingPanel.GetComponent<Animator>().SetBool("WindowOpen", false);
        WaitingPanel.GetComponent<Animator>().SetBool("Idle", false);
        this.GetComponent<TurnManager>().TurnPaused = false;

        if (!isServer)
        {

        }
        else
        {
            this.GetComponent<TurnManager>().ResetTurn();
            this.GetComponent<TurnManager>().RpcResetTurn();
        }
        closedUpTimer = true;
    }

    private void EndGameStartTimer()
    {
        CounterText.gameObject.GetComponent<Animator>().SetBool("DoAnimation", false);
        timerForGameStart = 0f;
        CounterText.gameObject.SetActive(false);
    }
}
