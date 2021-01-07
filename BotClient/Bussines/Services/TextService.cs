using BotClient.Bussines.Interfaces;
using BotClient.Models.Bot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BotClient.Bussines.Services
{
    public class TextService : ITextService
    {
        private readonly ISettingsService settingsService;


        public TextService(ISettingsService SettingsService)
        {
            settingsService = SettingsService;
        }

        private Random random = new Random();
        private List<string> randomMessages = new List<string>();

        public async Task<BotMessageText> RandOriginalMessage(string message)
        {
            var result = new BotMessageText();
            try
            {
                var maxAttept = random.Next(3, 6);
                for (int i = 0; i < maxAttept; i++)
                {
                    var randomMessage = await RandMessage(message).ConfigureAwait(false);
                    if (randomMessages.FirstOrDefault(item => item == randomMessage) == null)
                    {
                        randomMessages.Add(randomMessage);
                        result.Text = randomMessage;
                        break;
                    }
                }
                if(result.Text == null)
                    result.Text = await RandMessage(message).ConfigureAwait(false);
                result.TextParts = result.Text.Split('.').ToList();
                var errorChancePerTenWords = settingsService.GetServerSettings().ErrorChancePerTenWords;
                for (int i = 0; i < result.TextParts.Count; i++)
                {
                    if (result.TextParts[i].Length > 0)
                    {
                        var isSetedMessageKeyboardError = false;
                        if (random.Next(0, 100) < ((result.TextParts[i].Split(' ').Length / 10) * errorChancePerTenWords))
                        {
                            isSetedMessageKeyboardError = true;
                            var errorText = await SetMessageKeyboardError(result.TextParts[i]).ConfigureAwait(false);
                            result = SetCorrectWordId(result, result.TextParts[i], errorText);
                        }
                        var capsResult = await SetCaps(result.TextParts[i], isSetedMessageKeyboardError).ConfigureAwait(false);
                        if (String.Compare(result.TextParts[i], capsResult) == -1)
                            result.isHasCaps = true;
                        result.TextParts[i] = capsResult;
                        result.TextParts[i] = await ReplaceNumberToWord(result.TextParts[i]).ConfigureAwait(false);
                        result.TextParts[i] = RemoveBrackets(result.TextParts[i]);
                    }
                }
            }
            catch (Exception ex)
            {
                settingsService.AddLog("BotWorkService", ex);
            }
            return result;
        }

        public string TextToRegex(string Text)
        {
            try
            {
                var regexstres = Text.Split('\n');
                Text = "";
                for (int k = 0; k < regexstres.Length; k++)
                {
                    if (regexstres[k].Length > 0)
                    {
                        Text += "\\b" + regexstres[k] + "\\b|";
                    }
                }
                if (Text.Length > 0)
                    Text = Text.Remove(Text.Length - 1);
                return Text;
            }
            catch (Exception ex)
            {
                settingsService.AddLog("BotWorkService", ex);
            }
            return null;
        }

        private async Task<string> RandMessage(string message)
        {
            try
            {
                var RandomMessageConstantTextes = new List<MessageConstantText>();
                int LastOpen = 0;
                for (int i = 0; i < message.Length; i++)
                {
                    if (message[i] == '(')
                        LastOpen = i;
                    if ((message[i] == ')') && (LastOpen != -1))
                    {
                        var Elements = new List<string>();
                        Elements.Add("");
                        for (int j = LastOpen + 1; j < i; j++)
                        {
                            if (message[j] != '_')
                                Elements[Elements.Count - 1] += message[j];
                            else
                                Elements.Add("");
                        }
                        string oldstring = "(";
                        for (int j = 0; j < Elements.Count; j++)
                        {
                            oldstring += Elements[j] + "_";
                        }
                        oldstring = oldstring.Remove(oldstring.Length - 1);
                        oldstring += ")";
                        if (Elements.Count > 1)
                            message = message.Replace(oldstring, Elements[random.Next(0, Elements.Count)]);
                        else
                        {
                            var randomMessageConstantText = new MessageConstantText()
                            {
                                TextGuid = Guid.NewGuid(),
                                Text = $"({Elements[0]})"
                            };
                            RandomMessageConstantTextes.Add(randomMessageConstantText);
                            message = message.Replace(oldstring, randomMessageConstantText.TextGuid.ToString());
                        }
                        i = -1;
                    }
                }
                for (int i = 0; i < RandomMessageConstantTextes.Count; i++)
                    message = message.Replace(RandomMessageConstantTextes[i].TextGuid.ToString(), RandomMessageConstantTextes[i].Text);
                return message;
            }
            catch (Exception ex)
            {
                settingsService.AddLog("BotWorkService", ex);
            }
            return null;
        }
                
        private async Task<string> SetMessageKeyboardError(string Text)
        {
            var result = Text;
            try
            {
                var constantText = new List<MessageConstantText>();
                constantText = GetConstantTexts(Text, new Regex(@"(\(\w*\))"));
                Text = ReplaceConstantStringToKey(Text, constantText);
                var words = Text.Split(" ");
                var settingsErrorChancePerTenWords = settingsService.GetServerSettings().ErrorChancePerTenWords;
                char[][] keyboard = new char[3][]
                    {
                    new char[12] { 'й', 'ц', 'у', 'к', 'е', 'н', 'г', 'ш', 'щ', 'з', 'х', 'ъ' },
                    new char[11] { 'ф', 'ы', 'в', 'а', 'п', 'р', 'о', 'л', 'д', 'ж', 'э' },
                    new char[9] { 'я', 'ч', 'с', 'м', 'и', 'т', 'ь', 'б', 'ю' }
                    };
                var wordsIndexes = new List<int>();
                var regex = new Regex(@"\b[А-Яа-я]+\b", RegexOptions.IgnoreCase);
                if (words.Length >= 10)
                {
                    for (int i = 0; i < words.Length; i++)
                    {
                        if ((i > 0) && (i % 10 == 0) && (random.Next(0, 100) < settingsErrorChancePerTenWords))
                        {
                            var randomIndex = random.Next(i - 10, i);
                            if (regex.IsMatch(words[randomIndex]))
                                wordsIndexes.Add(randomIndex);
                        }
                    }
                }
                else if (random.Next(0, 100) < words.Length / settingsErrorChancePerTenWords)
                {
                    var randomIndex = random.Next(0, words.Length);
                    if (regex.IsMatch(words[randomIndex]))
                        wordsIndexes.Add(randomIndex);
                }
                for (int i = 0; i < wordsIndexes.Count; i++)
                {
                    if (regex.IsMatch(words[wordsIndexes[i]]))
                    {
                        var word = words[wordsIndexes[i]];
                        var characterIndex = random.Next(0, word.Length - 1);
                        for (int j = 0; j < keyboard.Length; j++)
                        {
                            for (int k = 0; k < keyboard[j].Length; k++)
                            {
                                if (word[characterIndex] == keyboard[j][k])
                                {
                                    var x = 0;
                                    var y = 0;
                                    if ((j + k) == 0)
                                    {
                                        x = random.Next(0, 2);
                                        y = random.Next(0, 2);
                                        if ((y == 0) && (x == 0))
                                            x = 1;
                                    }
                                    else if ((j == 0) && (k == 11))
                                    {
                                        y = random.Next(0, 2);
                                        x = 10;
                                    }
                                    else if ((j == 1) && (k == 0))
                                    {
                                        x = random.Next(0, 2);
                                        y = random.Next(0, 3);
                                    }
                                    else if ((j == 1) && (k == 10))
                                    {
                                        x = random.Next(9, 12);
                                        y = random.Next(0, 2);
                                        if ((y == 1) && (x != 9))
                                            x = 9;
                                    }
                                    else if ((j == 1) && (k == 9))
                                    {
                                        x = random.Next(8, 11);
                                        y = random.Next(0, 3);
                                        if ((y == 2) && (x > 8))
                                            x = 8;
                                    }
                                    else if ((j == 2) && (k == 0))
                                    {
                                        y = random.Next(1, 3);
                                        x = random.Next(0, 2);
                                    }
                                    else if ((j == 2) && (k == 8))
                                    {
                                        y = random.Next(1, 3);
                                        x = random.Next(7, 10);
                                        if (y == 2)
                                            x = 7;
                                    }
                                    else if (j == 0)
                                    {
                                        y = random.Next(0, 2);
                                        x = random.Next(k - 1, k + 2);
                                    }
                                    else if (j == 2)
                                    {
                                        y = random.Next(1, 3);
                                        x = random.Next(k - 1, k + 2);
                                    }
                                    else
                                    {
                                        y = random.Next(0, 3);
                                        x = random.Next(k - 1, k + 2);
                                    }
                                    StringBuilder sb = new StringBuilder(word);
                                    sb[characterIndex] = keyboard[y][x];
                                    word = sb.ToString();
                                }
                            }
                        }
                        words[wordsIndexes[i]] = word;
                    }
                }
                result = ConstructMessage(words.ToList());
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("BotWorkService", ex);
            }
            return result;
        }

        private BotMessageText SetCorrectWordId(BotMessageText OriginalText, string OriginalSentence, string ErrorSentence)
        {
            try
            {
                var originalWords = OriginalSentence.Split(" ").ToList();
                var errorWords = ErrorSentence.Split(" ").ToList();
                for (int i = 0; i < originalWords.Count; i++)
                {
                    if (errorWords.IndexOf(originalWords[i]) == -1)
                    {
                        var regex = new Regex(Regex.Escape(originalWords[i]));
                        var index = OriginalText.TextParts.IndexOf(OriginalSentence);
                        var id = Guid.NewGuid().ToString();
                        OriginalText.TextParts[index] = regex.Replace(OriginalSentence, id, 1);
                        OriginalText.isHasKeyboardError = true;
                        OriginalText.BotMessageErrorTexts.Add(new BotMessageErrorText()
                        {
                            CorrectText = originalWords[i],
                            CorrectTextIdString = id,
                            ErrorText = errorWords[i]
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                settingsService.AddLog("BotWorkService", ex);
            }
            return OriginalText;
        }

        private async Task<string> SetCaps(string Text, bool isSetedMessageKeyboardError)
        {
            var result = Text;
            try
            {
                var constantText = GetConstantTexts(Text, new Regex(@"(\(\w*\))"));
                Text = ReplaceConstantStringToKey(Text, constantText);
                var words = Text.Split(" ");
                var settingsCapsChancePerThousandWords = settingsService.GetServerSettings().CapsChancePerThousandWords;
                if (isSetedMessageKeyboardError)
                    settingsCapsChancePerThousandWords /= 2;
                if (settingsCapsChancePerThousandWords < 1)
                    settingsCapsChancePerThousandWords = 1;
                var regex = new Regex(@"\b[А-Яа-я]+\b");
                if (words.Length >= 100)
                {
                    for (int i = 0; i < words.Length; i++)
                    {
                        if ((i > 0) && (i % 100 == 0) && (random.Next(0, 100) < settingsCapsChancePerThousandWords))
                        {
                            var randomIndex = random.Next(0, words.Length);
                            if (regex.IsMatch(words[randomIndex]))
                                words[randomIndex] = words[randomIndex].ToUpper();
                        }
                    }
                }
                else if (random.Next(0, 100) < words.Length / settingsCapsChancePerThousandWords)
                {
                    var randomIndex = random.Next(0, words.Length);
                    if (regex.IsMatch(words[randomIndex]))
                        words[randomIndex] = words[randomIndex].ToUpper();
                }
                result = ConstructMessage(words.ToList());
            }
            catch (Exception ex)
            {
                settingsService.AddLog("BotWorkService", ex);
            }
            return result;
        }

        private async Task<string> ReplaceNumberToWord(string Text)
        {
            var result = Text;
            try
            {
                var constantText = GetConstantTexts(Text, new Regex(@"(\(\w*\))"));
                var textNumbers = GetConstantTexts(Text, new Regex(@"\b[0-9]{12}\b"));
                Text = ReplaceConstantStringToKey(Text, constantText);
                Text = ReplaceConstantStringToKey(Text, textNumbers);
                if (textNumbers.Count > 0)
                {
                    var numberChancePerHundredWords = settingsService.GetServerSettings().NumberChancePerHundredWords;
                    string[,] NumArray = {
                        {"один", "два", "три", "четыре", "пять", "шесть", "семь", "восемь", "девять"},
                        {"десять", "двадцать", "тридцать", "сорок", "пятьдесят", "шестьдесят", "семьдесят", "восемьдесят", "девяносто"},
                        {"сто", "двести", "триста", "четыреста", "пятьсот", "шестьсот", "семьсот", "восемьсот", "девятьсот"},
                        {"одиннадцать", "двенадцать", "тринадцать", "четырнадцать", "пятнадцать", "шестнадцать", "семнадцать", "восемнадцать", "девятнадцать" } };
                    string[,] TenArray = {
                        {"тысяча", "тысячи", "тысяч"},
                        {"миллион", "миллиона", "миллионов"},
                        {"миллиард", "миллиарда", "миллиардов"}};
                    for (int i = 0; i < numberChancePerHundredWords && textNumbers.Count > 0; i++)
                    {
                        var randomIndex = random.Next(0, textNumbers.Count);
                        var words = new List<string>();
                        for (int j = 0; j < textNumbers[randomIndex].Text.Length; j++)
                        {
                            var intValue = -1;
                            if ((int.TryParse(textNumbers[randomIndex].Text[j].ToString(), out intValue)) && (intValue > 0))
                            {
                                var numI = 2 - (j - (j / 3) * 3);
                                var tenI = 2 - (j / 3);
                                var tenJ = 0;
                                if ((numI == 0) && (words.Count > 0) && (words[words.Count - 1] == "десять"))
                                {
                                    words[words.Count - 1] = NumArray[3, intValue - 1];
                                    tenJ = 2;
                                }
                                else
                                {
                                    words.Add(NumArray[numI, intValue - 1]);
                                    if ((tenI == 0) && (numI == 0))
                                    {
                                        if (intValue == 1)
                                            words[words.Count - 1] = "одна";
                                        else if (intValue == 2)
                                            words[words.Count - 1] = "две";
                                    }
                                }
                                if ((j <= textNumbers[randomIndex].Text.Length - 3) && ((j + 1) % 3 == 0))
                                {
                                    if ((intValue >= 2) && (intValue <= 4) && (numI < 1))
                                        tenJ = 1;
                                    else if (intValue >= 5)
                                        tenJ = 2;
                                    words.Add(TenArray[tenI, tenJ]);
                                }
                            }
                        }
                        Text = Text.Replace(textNumbers[i].TextGuid.ToString(), String.Join(' ', words));
                        textNumbers.RemoveAll(item => item.TextGuid == textNumbers[i].TextGuid);
                    }
                }
                Text = ConstructMessage(Text, constantText);
                Text = ConstructMessage(Text, textNumbers);
                return Text;
            }
            catch (Exception ex)
            {
                settingsService.AddLog("BotWorkService", ex);
            }
            return result;
        }

        private List<MessageConstantText> GetConstantTexts(string Text, Regex RegulerExtension)
        {
            var result = new List<MessageConstantText>();
            var matches = RegulerExtension.Matches(Text);
            for (int i = 0; i < matches.Count; i++)
            {
                if (matches[i].Length > 0)
                {
                    var messageConstantText = new MessageConstantText()
                    {
                        Text = matches[i].Value,
                        TextGuid = Guid.NewGuid()
                    };
                    result.Add(messageConstantText);
                }
            }
            return result;
        }

        private string ReplaceConstantStringToKey(string Text, List<MessageConstantText> ConstantTexts)
        {
            for (int i = 0; i < ConstantTexts.Count; i++)
            {
                var regex = new Regex(@ConstantTexts[i].Text);
                Text = regex.Replace(Text, ConstantTexts[i].TextGuid.ToString(), 1);
            }
            return Text;
        }

        private string ConstructMessage(string Text, List<MessageConstantText> ConstantTexts)
        {
            for (int i = 0; i < ConstantTexts.Count; i++)
                Text = Text.Replace(ConstantTexts[i].TextGuid.ToString(), ConstantTexts[i].Text);
            return Text;
        }

        private string ConstructMessage(List<string> Words)
        {
            var result = "";
            for (int i = 0; i < Words.Count; i++)
                result += Words[i] + " ";
            result = result.Remove(result.Length - 1);
            return result;
        }

        private string RemoveBrackets(string Text)
        {
            Text = Text.Replace("(", "");
            Text = Text.Replace(")", "");
            return Text;
        }

    }
}
