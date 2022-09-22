using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.Internal;
using UnityEngine.UI;
using System;
using System.Globalization;
using Newtonsoft.Json;
using System.Dynamic;
using PlayFab.GroupsModels;
using PlayFab.DataModels;
using PlayFab.ProfilesModels;


public class PlayfabManager : MonoBehaviour
{
    public static PlayfabManager instance;
    public string displayName = string.Empty;
    public string playfabId = string.Empty;
    public bool logado = false;
    public bool justRegistered = false;
    public string titlePlayerId = string.Empty;
    public Dictionary<string, Dictionary<string, UserDataRecord>> retrievedUserData = new Dictionary<string, Dictionary<string, UserDataRecord>>();
    public Dictionary<string, string> titleData;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #region STATS

    public bool IsLoggedIn()
    {
        return PlayFabClientAPI.IsClientLoggedIn();
    }

    #endregion

    #region Registro e Login
    public void Register(string email, string username, string password, Action<RegisterPlayFabUserResult> Sucesso = null, Action<PlayFabError> Falha = null)
    {
       
        Debug.Log("PlayFab: Register");
        PlayFabClientAPI.RegisterPlayFabUser(
            // Request
            new RegisterPlayFabUserRequest
            {
                Username = username,
                Email = email,
                Password = password
            },
            // Success
            response =>
            {
                Debug.Log("PlayFab: OnRegisterSuccess");

                this.playfabId = response.PlayFabId;

                this.justRegistered = true;


                // Event
                if (Sucesso != null) Sucesso.Invoke(response);
            },
            // Failure
            error =>
            {
                Debug.LogError("PlayFab: OnRegisterFailure");

                // Event
                if (Falha != null) Falha.Invoke(error);
                Debug.LogError(error.GenerateErrorReport());

            }
        );
    }
    public void LoginWithCustomID(string email, string username, string TitleId, Action Sucesso = null, Action Falha = null)
    {
        if (PlayFabClientAPI.IsClientLoggedIn())
        {
            Debug.LogError("PlayFab: O cliente ja est� logado");
            return;
        }

        Debug.Log("PlayFab: Login");
        PlayFabClientAPI.LoginWithCustomID(
            // Request
            new LoginWithCustomIDRequest
            {
                CreateAccount = true,
                CustomId = email,
                TitleId = TitleId,
                InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
                {
                    GetPlayerProfile = true,
                    GetUserAccountInfo = true,
                    ProfileConstraints = new PlayerProfileViewConstraints()
                    {
                        ShowDisplayName = true,
                        ShowLastLogin = true,
                        ShowLocations = true
                    }
                }

            },
            // Success
            response =>
            {
                Debug.Log("PlayFab: Logado com Sucesso");

                this.playfabId = response.PlayFabId;
                this.displayName = username;
                logado = true;

                // Event
                if (Sucesso != null) Sucesso.Invoke();
            },
            // Failure
            error =>
            {
                Debug.LogError("PlayFab: Falha ao Logar");

                // Event
                if (Falha != null) Falha.Invoke();
                Debug.LogError(error.GenerateErrorReport());

            }
        );
    }
    public void LoginWithPlayFab(string username, string password, Action<LoginResult> Sucesso = null, Action<PlayFabError> Falha = null)
    {
        if (PlayFabClientAPI.IsClientLoggedIn())
        {
            Debug.LogError("PlayFab: O cliente ja esta logado");
            return;
        }

        Debug.Log("PlayFab: Login");
        PlayFabClientAPI.LoginWithPlayFab(
            // Request
            new LoginWithPlayFabRequest
            {
                Username = username,
                Password = password,
                InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
                {
                    GetPlayerProfile = true,
                    GetUserAccountInfo = true,
                    ProfileConstraints = new PlayerProfileViewConstraints()
                    {
                        ShowDisplayName = true
                    }
                }
            },
            // Success
            response =>
            {
                Debug.Log("PlayFab: Logado com Sucesso");

                this.playfabId = response.PlayFabId;
                if (response.InfoResultPayload != null)
                {
                    if (response.InfoResultPayload.PlayerProfile != null)
                    {
                        if (response.InfoResultPayload.PlayerProfile.DisplayName != null)
                        {
                            this.displayName = response.InfoResultPayload.PlayerProfile.DisplayName;
                        }
                    }
                }
                this.titlePlayerId = response.InfoResultPayload?.AccountInfo?.TitleInfo?.TitlePlayerAccount?.Id;
                // Event
                if (Sucesso != null) Sucesso.Invoke(response);
            },
            // Failure
            error =>
            {
                
                Debug.LogError("PlayFab: Falha ao Logar");

                // Event
                if (Falha != null) Falha.Invoke(error);
                Debug.LogError(error.GenerateErrorReport());

            }
        );
    }
    public void ChangeDisplayName(string displayName, Action Sucesso, Action Falha)
    {
        displayName = displayName.Trim();
        Debug.Log("PlayFab: ChangeDisplayName '" + displayName + "'...");
        PlayFabClientAPI.UpdateUserTitleDisplayName(
            // Request
            new UpdateUserTitleDisplayNameRequest
            {
                DisplayName = displayName
            },
            // Success
            result =>
            {
                this.displayName = displayName;
                Debug.Log("PlayFab: Changed player display name: " + result.DisplayName);

                // Event
                if (Sucesso != null) Sucesso.Invoke();
            },
            // Failure
            error =>
            {
                Debug.LogError("PlayFab: ChangeDisplayName failed");

                // Event
                if (Falha != null) Falha.Invoke();
            }
        );
    }

    public void SendAccountRecoveryEmail(string email, Action Sucesso = null, Action Falha = null)
    {
        Debug.Log("PlayFab: Register");
        PlayFabClientAPI.SendAccountRecoveryEmail(
            // Request
            new SendAccountRecoveryEmailRequest
            {

                Email = email,
                TitleId = "692BF"
            },
            // Success
            response =>
            {
                Debug.Log("PlayFab: OnRegisterSuccess");


                // Event
                if (Sucesso != null) Sucesso.Invoke();
            },
            // Failure
            error =>
            {
                Debug.LogError("PlayFab: OnRegisterFailure");

                // Event
                if (Falha != null) Falha.Invoke();
                Debug.LogError(error.GenerateErrorReport());

            }
        );
    }
    #endregion

    #region Contas
    public void GetUserData(string playfabId, Action<GetUserDataResult> onSuccess = null, Action<PlayFabError> onFailure = null)
    {
        
        Debug.Log($"PlayFab: GetUserData: {playfabId}");
        PlayFabClientAPI.GetUserData(
            // Request
            new GetUserDataRequest
            {
                PlayFabId = playfabId
            },
            // Success
            response =>
            {
                if (this.retrievedUserData.ContainsKey(playfabId))
                {
                    this.retrievedUserData[playfabId] = response.Data;
                }
                else
                {
                    this.retrievedUserData.Add(playfabId, response.Data);
                }

                Debug.Log("PlayFab: GetUserDataSuccess");

                // Event
                if (onSuccess != null) onSuccess.Invoke(response);
            },
            //Failure
            error =>
            {
                 
                Debug.Log("PlayFab: GetUserDataFailure");

                // Event
                if (onFailure != null) onFailure.Invoke(error);
            }
        );
    }
    public void UpdateUserData(string key, object value, Action Sucesso = null, Action Falha = null )
    {
        Dictionary<string, string> dataDictionary = new Dictionary<string, string>()
        {
            { key, value.ToString() }
        };
        UpdateUserData(dataDictionary, Sucesso, Falha );
    }

    public void UpdateUserData(Dictionary<string, string> newData, Action Sucesso = null, Action Falha = null )
    {
         
        Debug.Log("PlayFab: UpdateUserData");
        PlayFabClientAPI.UpdateUserData(
            // Request
            new UpdateUserDataRequest
            {
                Data = newData,
                Permission = UserDataPermission.Public
            },
            // Success
            response =>
            {
                 
                Debug.Log("PlayFab: UpdateUserDataSuccess");

                // Event
                if (Sucesso != null) Sucesso.Invoke();
            },
            // Failure
            error =>
            {
                 
                Debug.LogError("PlayFab: UpdateUserDataFaiure");
                Debug.LogError(error.GenerateErrorReport());

                // Event
                if (Falha != null) Falha.Invoke();
            }
        );
    }
    #region RETRIEVED_USER_DATA_FORMATTING
    // These methods should be called after "onGetUserDataSuccessEvent"
    // So it's guaranteed to have actual retrieved data
    public float GetFloatFromUserData(string key, string playfabId = "")
    {
        playfabId = playfabId == "" ? this.playfabId : playfabId;
        Dictionary<string, UserDataRecord> data = this.retrievedUserData.ContainsKey(playfabId) ? this.retrievedUserData[playfabId] : null;
        return data != null && data.ContainsKey(key) ? float.Parse(data[key].Value) : 0;
    }

    public int GetIntFromUserData(string key, string playfabId = "")
    {
        playfabId = playfabId == "" ? this.playfabId : playfabId;
        Dictionary<string, UserDataRecord> data = this.retrievedUserData.ContainsKey(playfabId) ? this.retrievedUserData[playfabId] : null;
        return data != null && data.ContainsKey(key) ? int.Parse(data[key].Value) : 0;
    }

    public string GetStringFromUserData(string key, string playfabId = "")
    {
        playfabId = playfabId == "" ? this.playfabId : playfabId;
        Dictionary<string, UserDataRecord> data = this.retrievedUserData.ContainsKey(playfabId) ? this.retrievedUserData[playfabId] : null;
        return data != null && data.ContainsKey(key) ? data[key].Value : string.Empty;
    }

    public bool GetBoolFromUserData(string key, string playfabId = "")
    {
        playfabId = playfabId == "" ? this.playfabId : playfabId;
        Dictionary<string, UserDataRecord> data = this.retrievedUserData.ContainsKey(playfabId) ? this.retrievedUserData[playfabId] : null;
        if (data != null && data.ContainsKey(key) && data[key].Value != string.Empty)
        {
            try
            {
                return Convert.ToBoolean(data[key].Value);
            }
            catch
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    #endregion
    #endregion

    #region Ranking
    public void UpdateStatistic(string stat, int value, bool showLoadingScreen = false, Action Sucesso = null, Action Falha = null)
    {
         
        Debug.Log("PlayFab: UpdateStatistic '" + stat + "'...");
        PlayFabClientAPI.UpdatePlayerStatistics(
            // Request
            new UpdatePlayerStatisticsRequest
            {
                Statistics = new List<StatisticUpdate>
                {
                    new StatisticUpdate {StatisticName = stat, Value = value}
                }
            },
            // Success
            response =>
            {
                 
                Debug.Log("PlayFab: UpdateStatistic completed");
                if (Sucesso != null) Sucesso.Invoke();
            },
            // Failure
            error =>
            {
                 
                Debug.LogError("PlayFab: UpdateStatistic failed");
                Debug.LogError(error.GenerateErrorReport());
                if (Falha != null) Falha.Invoke();
            }
        );
    }
    public List<PlayerLeaderboardEntry> retrievedLeaderboard = new List<PlayerLeaderboardEntry>();
    public void GetLeaderboard(string statisticName, bool showLoadingScreen = false, Action<GetLeaderboardResult> Sucesso = null, Action<PlayFabError> Falha = null)
    {
         
        Debug.Log("PlayFab: GetLeaderboard '" + statisticName + "'...");
        PlayFabClientAPI.GetLeaderboard(
            // Request
            new GetLeaderboardRequest
            {
                StatisticName = statisticName,
                StartPosition = 0,
                MaxResultsCount = 100
            },
            // Success
            result =>
            {
                 
                Debug.Log("PlayFab: GetLeaderboard completed");
                this.retrievedLeaderboard = result.Leaderboard;

                // Event

                if (Sucesso != null) Sucesso.Invoke(result);
            },
            // Failure
            error =>
            {
                 
                Debug.LogError("PlayFab: GetLeaderboard failed");
                Debug.LogError(error.GenerateErrorReport());

                // Event
                if (Falha != null) Falha.Invoke(error);
            }
        );
    }
    public PlayerLeaderboardEntry retrievedPlayerRankEntry;

    public void GetLeaderboardAroundPlayer(string statisticName, Action<GetLeaderboardAroundPlayerResult> Sucesso = null, Action<PlayFabError> Falha = null, bool showLoadingScreen = false)
    {
         
        Debug.Log("PlayFab: GetLeaderboardAroundPlayer '" + statisticName + "'...");
        PlayFabClientAPI.GetLeaderboardAroundPlayer(
            // Request
            new GetLeaderboardAroundPlayerRequest
            {
                StatisticName = statisticName,
                MaxResultsCount = 1
            },
            // Success
            result =>
            {
                 
                Debug.Log("PlayFab: GetLeaderboardAroundPlayer completed");

                this.retrievedPlayerRankEntry = result.Leaderboard[0];

                // Event
                if (Sucesso != null) Sucesso.Invoke(result);
            },
            // Failure
            error =>
            {
                 
                Debug.LogError("PlayFab: GetLeaderboardAroundPlayer failed");
                Debug.LogError(error.GenerateErrorReport());

                // Event
                if (Falha != null) Falha.Invoke(error);
            }
        );
    }
    #endregion

    #region Eventos
    public void GetTitleData(Action Sucesso = null, Action Falha = null )
    {
        Debug.Log("Playfab: GetTitleData");
         
        PlayFabClientAPI.GetTitleData(
            // Request
            new GetTitleDataRequest()
            {

            },
            // Success
            response =>
            {
                Debug.Log("Playfab: GetTitleDataSuccess");

                 

                this.titleData = response.Data;

                // Event
                if (Sucesso != null) Sucesso.Invoke();
            },
            // Failure
            error =>
            {
                Debug.LogError("Playfab: GetTitleDataFailure:");
                Debug.LogError(error.GenerateErrorReport());

                 

                // Event
                if (Falha != null) Falha.Invoke();
            }
        );
    }
    public void WriteTitleEvent(string eventName, Dictionary<string, object> body = null, Action Sucesso = null, Action Falha = null)
    {
        Debug.Log("Playfab: WriteTitleEvent: " + eventName);

        if (body == null)
        {
            body = new Dictionary<string, object>();
        }
        body.Add("GameVersion", Application.version);
        body.Add("DisplayName", this.displayName);

        PlayFabClientAPI.WriteTitleEvent(
            // Request
            new WriteTitleEventRequest()
            {
                EventName = eventName,
                Body = body
            },
            // Success
            result =>
            {
                Debug.Log("Playfab: WriteTitleEventSuccess");

                // Event
                if (Sucesso != null) Sucesso.Invoke();
            },
            // Failure
            error =>
            {
                Debug.LogError("Playfab: WriteTitleEventError:");
                Debug.LogError(error.GenerateErrorReport());

                // Event
                if (Falha != null) Falha.Invoke();
            }
        );
    }
    public void WritePlayerEvent(string eventName, Dictionary<string, object> body = null, Action Sucesso = null, Action Falha = null)
    {
        Debug.Log("Playfab: WritePlayerEvent: " + eventName);

        if (body == null)
        {
            body = new Dictionary<string, object>();
        }
        body.Add("GameVersion", Application.version);
        if (!body.ContainsKey("DisplayName"))
        {
            body.Add("DisplayName", this.displayName);
        }

        PlayFabClientAPI.WritePlayerEvent(
            // Request
            new WriteClientPlayerEventRequest()
            {
                EventName = eventName,
                Body = body
            },
            // Success
            result =>
            {
                Debug.Log("Playfab: WritePlayerEventSuccess");

                // Event
                if (Sucesso != null) Sucesso.Invoke();
            },
            // Failure
            error =>
            {
                Debug.LogError("Playfab: WritePlayerEventError:");
                Debug.LogError(error.GenerateErrorReport());

                // Event
                if (Falha != null) Falha.Invoke();
            }
        );
    }
    #endregion

    #region DataPerguntas
    //public void RespondeuPergunta(int idPergunta, int progressAmount = 1, Action Sucesso = null, Action Falha = null)
    //{
    //    Debug.Log($"PlayFab: Respondeu a pergunta {idPergunta}");
    //    RespondeuPerguntas(new List<int>() { idPergunta }, progressAmount, Sucesso, Falha);
    //}

    //public void RespondeuPerguntas(List<int> idPergunta, int progressAmount = 1, Action SucessoPerguntas = null, Action FalhaPerguintas = null)
    //{
    //    Debug.Log($"PlayFab: Respondeu as perguntas");
    //    GetUserData(this.playfabId,
    //                Sucesso =>
    //                {
    //                    //Pegando as Perguntas do Player
    //                    string perguntasJson = GetStringFromUserData("Perguntas");
    //                    List<PerguntasModel> perguntas = new List<PerguntasModel>();

    //                    if (perguntasJson != string.Empty)
    //                    {
    //                        perguntas = JsonConvert.DeserializeObject<List<PerguntasModel>>(perguntasJson);
    //                    }

    //                    foreach (int id in idPergunta)
    //                    {
    //                        foreach (PerguntasModel pergunta in perguntas)
    //                        {
    //                            if (pergunta.Id == id)
    //                            {
    //                                pergunta.VezesQueRespondeu += progressAmount;
    //                            }
    //                        }
    //                    }

    //                    // Uploading data
    //                    string serializedData = JsonConvert.SerializeObject(perguntas);
    //                    UpdateUserData("Perguntas", serializedData,
    //                                    () =>
    //                                    {
    //                                        Debug.Log($"PlayFab: Upload do progresso das perguntas com Sucesso");
    //                                        if (SucessoPerguntas != null) SucessoPerguntas.Invoke();
    //                                    },
    //                                    () =>
    //                                    {
    //                                        Debug.LogError($"PlayFab: Upload do progresso das perguntas Falhou");
    //                                        if (FalhaPerguintas != null) FalhaPerguintas.Invoke();

    //                                    }, false);
    //                },
    //                Falha =>
    //                {
    //                    Debug.LogError("Playfab: Falhou em Pegar o data do player nas perguntas");

    //                }, false);


    //}
    #endregion

    #region Amigos
    public string amigoFalha;
    public void AddFriend(string displayName, Action<AddFriendResult> Sucesso = null, Action<PlayFabError> Falha = null)
    {
        AddFriendToThisUser();

        void AddFriendToThisUser()
        {
            
            Debug.Log("Playfab: AddFriend");
            PlayFabClientAPI.AddFriend(
                // Request
                new AddFriendRequest()
                {
                    FriendTitleDisplayName = displayName
                },
                // Success
                response =>
                {
                     
                    Debug.Log("Playfab: AddFriendSuccess");

                    // Event
                    if (Sucesso != null) Sucesso.Invoke(response);
                },
                // Failure
                error =>
                {

                     
                    Debug.LogError("Playfab: AddFriendError");
                    Debug.LogError(error.GenerateErrorReport());
                    // Event
                    if (Falha != null) Falha.Invoke(error);
                }
            );
        }
    }
    #endregion
}
