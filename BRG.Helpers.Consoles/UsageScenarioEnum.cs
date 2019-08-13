using System;

namespace BRG.Helpers.Consoles
{
    /// <summary>
    /// In quale scenario è stato istanziata la Console. Determina il comportamento nella gestione di task asincroni generati dalla classe.
    /// </summary>
    public enum UsageScenarioEnum
    {
        // La chiusura del task principale troncherà tutti i sottotask asincroni lanciati e ancora in esecuzione (è necessario aspettare esplicitamente che terminino)
        CONSOLE_APP,           // Es: eseguibile .exe schedulato 
        WEB_JOB,               // Es: Azure Web Job

        // La chiusura del task principale ritornerà l'esecuzione al task di sistema mentre i sottotask asincroni continueranno fino a conclusione (non serve aspettare)
        WEB_APP,               // Es: Sito deployato come Azure Web App Service
        WEB_SERVICE,           // Es: Webservice .svc SOAP/REST
        UI_APP                 // Es: desktop application
    }
}
