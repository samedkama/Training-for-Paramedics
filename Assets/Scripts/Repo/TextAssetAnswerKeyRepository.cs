using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using VR.Triage.Models;
using VR.Triage.Repo;


namespace VR.Triage.Repo
{
    public interface IAnswerKeyRepository
    {
        Task<AnswerKeyDefinition> LoadAnswerKeyAsync(string scenarioId);
    }

    public class TextAssetAnswerKeyRepository : IAnswerKeyRepository
    {
        public async Task<AnswerKeyDefinition> LoadAnswerKeyAsync(string scenarioId)
        {
            // Resources.Load nutzt Pfade relativ zu Assets/Resources UND ohne Dateiendung
            var ta = Resources.Load<TextAsset>($"AnswerKeys/{scenarioId}");

            if (ta == null)
                throw new System.Exception($"AnswerKey not found: Resources/AnswerKeys/{scenarioId}.json");

            await Task.Yield(); // simuliert async (wie bei deinem Case-Repo)
            return JsonConvert.DeserializeObject<AnswerKeyDefinition>(ta.text);
        }
    }
}
