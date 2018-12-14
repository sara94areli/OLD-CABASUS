using Android.Util;
using WindowsAzure.Messaging;
using Firebase.Iid;
using Android.App;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Dynamic;
using static Android.Provider.Settings;
using System.Text;
using System;

namespace Cabasus_Android
{
	[Service]
    [IntentFilter(new[] { "com.google.firebase.INSTANCE_ID_EVENT" })]   
	public class MyFirebaseIIDService : FirebaseInstanceIdService
    {
		const string TAG = "MyFirebaseIIDService";
        NotificationHub hub;
        ShareInside CG = new ShareInside();

        public MyFirebaseIIDService()
        {
            OnTokenRefresh();
        }

        public override void OnTokenRefresh()
        {
            var refreshedToken = FirebaseInstanceId.Instance.Token;
            Log.Debug(TAG, "FCM token: " + refreshedToken);
            SendRegistrationToServer(refreshedToken);
        }

        void SendRegistrationToServer(string token)
        {
            // Register with Notification Hubs
            try
            {
                hub = new NotificationHub(Constants.NotificationHubName,
                          Constants.ListenConnectionString, this);

                var tags = new List<string>() { };
                var regID = hub.Register(token, tags.ToArray()).RegistrationId;

                Log.Debug(TAG, $"Successful registration of ID {regID}");
                UpdatePhone(regID);
            }
            catch (Exception)
            {
                UpdatePhone(token);
            }

        }

        private async void UpdatePhone(string token)
        {
            try
            {
                string server = "https://cabasus-mobile.azurewebsites.net/v1/profile";
                HttpClient cliente = new HttpClient();
                string ContentType = "application/json";
                cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(CG.ConsultToken());
                var respuestaActualizar = await cliente.GetAsync(server);
                if (respuestaActualizar.IsSuccessStatusCode)
                {
                    var ContenidoUpdate = await respuestaActualizar.Content.ReadAsStringAsync();
                    var ValoresPhone = JsonConvert.DeserializeObject<GetUser>(ContenidoUpdate);
                    ValoresPhone.photo.content_type = "image/jpeg";
                    dynamic ObjetosCreatePhone = new ExpandoObject();
                    if (ValoresPhone.phones.Count <= 0)
                    {
                        ObjetosCreatePhone.number = "1";
                        ObjetosCreatePhone.os = "Android";
                        ObjetosCreatePhone.token = token;
                    }
                    else
                    {
                        ObjetosCreatePhone.number = ValoresPhone.phones[0].number;
                        ObjetosCreatePhone.os = "Android";
                        ObjetosCreatePhone.token = token;
                        ObjetosCreatePhone.registrationId = ValoresPhone.phones[0].registrationId;
                    }
                    string JsonUserPhone = JsonConvert.SerializeObject(ObjetosCreatePhone, Formatting.Indented, new JsonSerializerSettings());
                    server = "https://cabasus-mobile.azurewebsites.net/v1/profile/phones";

                    var RespuestaCreatePhone = await cliente.PostAsync(server, new StringContent(JsonUserPhone, Encoding.UTF8, ContentType));
                    RespuestaCreatePhone.EnsureSuccessStatusCode();
                    if (RespuestaCreatePhone.IsSuccessStatusCode)
                    {
                        var ContenidoCreatePhone = await RespuestaCreatePhone.Content.ReadAsStringAsync();
                        TokenPhone deserializedProduct = JsonConvert.DeserializeObject<TokenPhone>(ContenidoCreatePhone);
                        CG.SaveTokenPhone(deserializedProduct.id);
                    }
                    else
                    {

                    }
                }
            }
            catch (Exception ex)
            {
            }

        }
    }
}
