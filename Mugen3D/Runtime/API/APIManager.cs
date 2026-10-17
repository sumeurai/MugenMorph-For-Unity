using Mugen3D.Core;
using Mugen3D.MugenMorph;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


namespace Mugen3D.API
{
    public class APIManager : MonoBehaviour
    {
        private static APIManager instance;

        public static APIManager Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject o = new GameObject("APIManager");
                    instance = o.AddComponent<APIManager>();
                }
                return instance;
            }
        }

        [SerializeField] private string accessToken;

        public string AccessToken
        {
            get
            {
                return accessToken;
            }
            private set
            {
                accessToken = value;
            }
        }


        private BaseHttpRequest httpRequest;

        public BaseHttpRequest HttpRequest
        {
            get
            {
                if (httpRequest == null)
                {
                    httpRequest = new BaseHttpRequest();
                }
                return httpRequest;
            }
        }


        private void Awake()
        {
            DontDestroyOnLoad(this);
        }


        public async Task<TRep> RequestAsync<TReq, TRep>(
            string url,
            TReq requestData,
            Action<TRep> onSuccess = null,
            Action<string> onError = null)
            where TRep : BaseResponse
        {
            try
            {
                //string token = url == APISettingsConfig.Instance.LoginUrl ? "" : AccessToken;
                string token = url == APISettingsConfig.Instance.LoginUrl ? "" : AccessToken;
                token = AccessToken;
                
                TRep response = null;
                await HttpRequest.PostAsync<TReq, TRep>(
                    url,
                    token,
                    requestData,
                    (data) =>
                    {
                        response = data;
                        onSuccess?.Invoke(data);
                    },
                    (error) =>
                    {
                        Debug.LogError($"[API Error] {url}: {error}");
                        onError?.Invoke(error);
                    }
                );

                return response;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[API Exception] {url}: {ex.Message}");
                onError?.Invoke(ex.Message);
                return null;
            }
        }

        public void Request<TReq, TRep>(
            string url,
            TReq requestData,
            Action<TRep> onSuccess = null,
            Action<string> onError = null)
            where TRep : BaseResponse
        {
            _ = RequestAsync<TReq, TRep>(url, requestData, onSuccess, onError);
        }



        public void Login()
        {
            if (!APISettingsConfig.Instance.IsValid())
            {
                Debug.LogError("[API Error] accesskey or secretkey is empty!");
                return;
            }

            LoginReqData reqData = new LoginReqData();
            reqData.accessKey = APISettingsConfig.Instance.AccessKey;
            reqData.secretKey = APISettingsConfig.Instance.SecretKey;

            Request<LoginReqData, LoginRepData>(APISettingsConfig.Instance.LoginUrl,reqData, (repData) =>
            {
                AccessToken = repData.data.accessToken;
            });
        }

        public void RequestMugenMorph(byte[] audioBytes, Action<MugenMorphProtobufResult> onSuccess = null, Action<string> onError = null)
        {
            _ = RequestMugenMorphAsync(audioBytes, onSuccess, onError);
        }

        public async Task<MugenMorphProtobufResult> RequestMugenMorphAsync(byte[] audioBytes, Action<MugenMorphProtobufResult> onSuccess = null, Action<string> onError = null)
        {
            if (audioBytes == null || audioBytes.Length == 0)
            {
                onError?.Invoke("audio is empty");
                return null;
            }

            MugenMorphReqData reqData = new MugenMorphReqData();
            reqData.traceId = Guid.NewGuid().ToString("N");
            reqData.data = Convert.ToBase64String(audioBytes);

            string json = JsonUtility.ToJson(reqData);
            byte[] body = Encoding.UTF8.GetBytes(json);
            string url = APISettingsConfig.Instance.MugenMorphMeshUrl;

            Debug.Log($"[MugenMorph] POST {url} protobuf=true, traceId={reqData.traceId}");

            MugenMorphProtobufResult result = null;
            await HttpRequest.PostRawAsync(
                url,
                AccessToken,
                body,
                "application/x-protobuf",
                (bytes, text, contentType) =>
                {
                    try
                    {
                        result = ParseMugenMorphResponse(bytes, text, contentType);
                        if (result != null && result.Code != 0 && result.Code != 200)
                        {
                            onError?.Invoke(string.IsNullOrEmpty(result.Message) ? $"MugenMorph code={result.Code}" : result.Message);
                            result = null;
                            return;
                        }

                        onSuccess?.Invoke(result);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[MugenMorph] parse failed: {ex}");
                        onError?.Invoke(ex.Message);
                    }
                },
                error =>
                {
                    Debug.LogError($"[MugenMorph] Request failed: {error}");
                    onError?.Invoke(error);
                });

            return result;
        }

        private static MugenMorphProtobufResult ParseMugenMorphResponse(byte[] bytes, string text, string contentType)
        {
            int bodyLength = bytes != null ? bytes.Length : 0;
            Debug.Log($"[MugenMorph] protobuf body={bodyLength} bytes, contentType={contentType}, head={MugenMorphProtobufParser.DescribeHead(bytes)}");

            if (bytes != null && bytes.Length > 0 && bytes[0] == (byte)'{')
            {
                MugenMorphRepData repData = JsonUtility.FromJson<MugenMorphRepData>(text);
                if (repData == null || repData.data == null)
                {
                    throw new InvalidOperationException("JSON response missing data");
                }

                MugenMorphProtobufResult jsonResult = new MugenMorphProtobufResult();
                jsonResult.Code = repData.code;
                jsonResult.Message = repData.message;
                jsonResult.Fps = repData.data.fps;
                jsonResult.Audio = DecodeBase64OrNull(repData.data.audioKey);
                jsonResult.Blendshapes = DecodeBase64OrNull(repData.data.emoteKey);
                return jsonResult;
            }

            return MugenMorphProtobufParser.Parse(bytes);
        }

        private static byte[] DecodeBase64OrNull(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            return Convert.FromBase64String(value);
        }


        public string ComputeStringMD5(string s)
        {
            // Create MD5 hash creator
            MD5 hashCreator = MD5.Create();

            // Convert the input string to a byte array
            byte[] data = hashCreator.ComputeHash(Encoding.UTF8.GetBytes(s));

            // Create a StringBuilder to collect the bytes and create a string
            StringBuilder stringBuilder = new StringBuilder();

            // Loop through each byte of the hashed data and format each one as a hexadecimal string
            for (int i = 0; i < data.Length; i++)
            {
                stringBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string
            return stringBuilder.ToString();
        }



    }
}
