using System;
using System.Collections;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using GFD.Quest;

namespace GFD.Utilities
{
    public static class GeminiQuestGenerator
    {
        private static readonly string apiKey = ""; // Store this securely, perhaps in a config file
        private static readonly string geminiApiEndpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent";

        [Serializable]
        private class GeminiRequest
        {
            public Content[] contents;
            public GenerationConfig generationConfig;

            public GeminiRequest(string prompt)
            {
                contents = new Content[] {
                    new Content {
                        parts = new Part[] {
                            new Part { text = prompt }
                        }
                    }
                };

                generationConfig = new GenerationConfig
                {
                    temperature = 0.7f,
                    maxOutputTokens = 800,
                    topP = 0.95f
                };
            }
        }

        [Serializable]
        private class GenerationConfig
        {
            public float temperature;
            public int maxOutputTokens;
            public float topP;
        }

        [Serializable]
        private class Content
        {
            public Part[] parts;
        }

        [Serializable]
        private class Part
        {
            public string text;
        }

        [Serializable]
        private class GeminiResponse
        {
            public Candidate[] candidates;
        }

        [Serializable]
        private class Candidate
        {
            public Content content;
            public string finishReason;
        }

        // Method for MonoBehaviours to call via coroutine
        public static IEnumerator GenerateQuest(
            QuestParameters parameters,
            Action<QuestData> onComplete)
        {
            string prompt = BuildQuestPrompt(parameters);

            yield return ProcessWithGemini(prompt, response => {
                // Validate and structure the response
                QuestData quest = ValidateAndParseResponse(response, parameters);
                onComplete?.Invoke(quest);
            });
        }

        // Builds the prompt with self-assessment instructions
        private static string BuildQuestPrompt(QuestParameters parameters)
        {
            StringBuilder prompt = new StringBuilder();

            prompt.AppendLine("Generate a fantasy quest for a guild management game with self-assessment.");
            prompt.AppendLine("\nINSTRUCTIONS:");
            prompt.AppendLine("1. Create a quest with the specified parameters");
            prompt.AppendLine("2. Evaluate if the quest meets all constraints");
            prompt.AppendLine("3. If constraints are not met, fix the quest to meet them");
            prompt.AppendLine("4. Return only the final quest in JSON format");

            prompt.AppendLine("\nPARAMETERS:");
            prompt.AppendLine($"- Difficulty: {parameters.difficulty}");
            prompt.AppendLine($"- Quest Type: {parameters.preferredType}");

            prompt.AppendLine("\nCONSTRAINTS:");
            prompt.AppendLine("- Quest name must be 2-6 words");
            prompt.AppendLine("- Description must be 1-3 sentences and under 200 characters");
            prompt.AppendLine("- Quest type must be one of: Elimination, Exploration, Escort, Retrieval, Assassination");
            prompt.AppendLine("- Reward and duration should be appropriate for difficulty");
            prompt.AppendLine("- Content should be fantasy-themed and appropriate for the quest type");

            prompt.AppendLine("\nOUTPUT FORMAT:");
            prompt.AppendLine("{");
            prompt.AppendLine("  \"name\": \"Quest Name\",");
            prompt.AppendLine("  \"description\": \"Quest description text\",");
            prompt.AppendLine("  \"type\": \"QuestType\",");
            prompt.AppendLine("  \"reward\": 100,");
            prompt.AppendLine("  \"duration\": 2");
            prompt.AppendLine("}");

            return prompt.ToString();
        }

        // Communicates with Gemini API
        private static IEnumerator ProcessWithGemini(string prompt, Action<string> callback)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                Debug.LogError("API key not set for Gemini integration");
                callback?.Invoke(null);
                yield break;
            }

            string url = $"{geminiApiEndpoint}?key={apiKey}";
            var request = new GeminiRequest(prompt);
            string jsonBody = JsonUtility.ToJson(request);

            using (UnityWebRequest webRequest = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
                webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "application/json");

                yield return webRequest.SendWebRequest();

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"Gemini API Error: {webRequest.error}");
                    callback?.Invoke(null);
                    yield break;
                }

                try
                {
                    var response = JsonUtility.FromJson<GeminiResponse>(webRequest.downloadHandler.text);
                    if (response.candidates != null && response.candidates.Length > 0 &&
                        response.candidates[0].content != null &&
                        response.candidates[0].content.parts != null &&
                        response.candidates[0].content.parts.Length > 0)
                    {
                        callback?.Invoke(response.candidates[0].content.parts[0].text);
                    }
                    else
                    {
                        Debug.LogError("Empty or invalid response from Gemini");
                        callback?.Invoke(null);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to parse Gemini response: {e.Message}");
                    callback?.Invoke(null);
                }
            }
        }

        // Validates and parses the Gemini response
        private static QuestData ValidateAndParseResponse(string geminiResponse, QuestParameters parameters)
        {
            if (string.IsNullOrEmpty(geminiResponse))
            {
                return CreateFallbackQuest(parameters);
            }

            try
            {
                // Extract JSON from the response (in case Gemini added any commentary)
                string jsonContent = ExtractJsonFromResponse(geminiResponse);

                // Parse the JSON content
                QuestResponseFormat responseData = JsonUtility.FromJson<QuestResponseFormat>(jsonContent);

                // Validate parsed data
                if (string.IsNullOrEmpty(responseData.name) ||
                    string.IsNullOrEmpty(responseData.description) ||
                    string.IsNullOrEmpty(responseData.type))
                {
                    Debug.LogWarning("Gemini response missing required fields");
                    return CreateFallbackQuest(parameters);
                }

                // Create and return the QuestData
                var questData = new QuestData
                {
                    questId = Guid.NewGuid().ToString(),
                    questName = responseData.name,
                    description = responseData.description,
                    questType = ParseQuestType(responseData.type),
                    difficulty = parameters.difficulty,
                    status = QuestStatus.Pending,
                    reward = responseData.reward > 0 ? responseData.reward : CalculateReward(parameters.difficulty),
                    duration = responseData.duration > 0 ? responseData.duration : GetMaxDuration(parameters.difficulty),
                    assignedAdventurerId = String.Empty
                };

                return questData;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to parse Gemini response: {e.Message}\nResponse: {geminiResponse}");
                return CreateFallbackQuest(parameters);
            }
        }

        // Helper class for parsing the JSON response
        [Serializable]
        private class QuestResponseFormat
        {
            public string name;
            public string description;
            public string type;
            public int reward;
            public int duration;
        }

        private static string ExtractJsonFromResponse(string response)
        {
            int startIndex = response.IndexOf('{');
            int endIndex = response.LastIndexOf('}');

            if (startIndex >= 0 && endIndex >= 0 && endIndex > startIndex)
            {
                return response.Substring(startIndex, endIndex - startIndex + 1);
            }
            return response;
        }

        // Parse quest type from string
        private static QuestType ParseQuestType(string typeString)
        {
            if (Enum.TryParse<QuestType>(typeString, true, out var result))
                return result;

            return QuestType.Elimination; // Default
        }

        // Calculate reward based on difficulty
        private static int CalculateReward(QuestDifficulty difficulty)
        {
            return difficulty switch
            {
                QuestDifficulty.Easy => UnityEngine.Random.Range(50, 100),
                QuestDifficulty.Normal => UnityEngine.Random.Range(100, 200),
                QuestDifficulty.Hard => UnityEngine.Random.Range(200, 400),
                QuestDifficulty.Deadly => UnityEngine.Random.Range(400, 800),
                _ => 100
            };
        }

        // Calculate duration based on difficulty
        private static int GetMaxDuration(QuestDifficulty difficulty)
        {
            return difficulty switch
            {
                QuestDifficulty.Easy => UnityEngine.Random.Range(1, 3),
                QuestDifficulty.Normal => UnityEngine.Random.Range(2, 4),
                QuestDifficulty.Hard => UnityEngine.Random.Range(3, 6),
                QuestDifficulty.Deadly => UnityEngine.Random.Range(4, 8),
                _ => 2
            };
        }

        // Create a fallback quest when generation fails
        private static QuestData CreateFallbackQuest(QuestParameters parameters)
        {
            return new QuestData
            {
                questId = Guid.NewGuid().ToString(),
                questName = $"Emergency {parameters.preferredType} Quest",
                description = $"A standard {parameters.difficulty.ToString().ToLower()} {parameters.preferredType.ToString().ToLower()} mission.",
                questType = parameters.preferredType,
                difficulty = parameters.difficulty,
                status = QuestStatus.Pending,
                reward = CalculateReward(parameters.difficulty),
                duration = GetMaxDuration(parameters.difficulty),
            };
        }
    }

    // Parameters class for quest generation
    [Serializable]
    public class QuestParameters
    {
        public QuestDifficulty difficulty = QuestDifficulty.Normal;
        public QuestType preferredType = QuestType.Elimination;
    }
}
