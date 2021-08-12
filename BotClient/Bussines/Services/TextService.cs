using BotClient.Bussines.Interfaces;
using BotClient.Models.Bot;
using BotDataModels.Bot.Enumerators;
using BotDataModels.Enumerators;
using BotDataModels.Role;
using BotMySQL.Bussines.Interfaces.MySQL;
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
        private readonly IPhraseService phraseService;

        public TextService(ISettingsService SettingsService, 
                           IPhraseService PhraseService)
        {
            settingsService = SettingsService;
            phraseService = PhraseService;
        }

        private Random random = new Random();
        private List<string> randomMessages = new List<string>();

        public async Task<BotMessageText> RandOriginalMessage(string message, string Name = null, EnumGender? Gender = 0)
        {
            var result = new BotMessageText();
            try
            {
                var settings = settingsService.GetServerSettings();/*
                var maxAttept = random.Next(settings.MinAtteptCountToRandMessage, settings.MaxAtteptCountToRandMessage);
                for (int i = 0; i < maxAttept; i++)
                {
                    var randomMessage = await RandMessage(message).ConfigureAwait(false);
                    if (randomMessages.FirstOrDefault(item => item == randomMessage) == null)
                    {
                        randomMessages.Add(randomMessage);
                        result.Text = randomMessage;
                        break;
                    }
                }*/
                var constantText = await GetURLS(message).ConfigureAwait(false);
                message = ReplaceConstantStringToKey(message, constantText);
                constantText = await UpdateLinks(constantText).ConfigureAwait(false);
                if (result.Text == null)
                    result.Text = await RandMessage(message).ConfigureAwait(false);
                constantText.AddRange(GetConstantTexts(result.Text));
                result.Text = ReplaceConstantStringToKey(result.Text, constantText);
                result.Text = await SetPhrases(result.Text).ConfigureAwait(false);
                result.Text = await ReplaceSecondaryText(result.Text).ConfigureAwait(false);
                result.Text = await SetContact(Name, Gender, result.Text);
                result.Text = Regex.Replace(result.Text, @"\s+", " ");
                var textParts = new List<string>();
                var splitChance = 0;
                var text = "";
                if ((settings.PlotCommaSplitChance > 0) && (settings.SpaceSplitChance > 0))
                {
                    for (int i = 0; i < result.Text.Length; i++)
                    {
                        text += result.Text[i];
                        if ((result.Text[i] == '.') || (result.Text[i] == ','))
                        {
                            splitChance += settings.PlotCommaSplitChance;
                            if (random.Next(0, 100) < splitChance)
                            {
                                textParts.Add(text);
                                text = "";
                                splitChance = 0;
                            }
                        }
                    }
                    splitChance = 0;
                    if (textParts.IndexOf(text) == -1)
                        textParts.Add(text);
                    for (int i = 0; i < textParts.Count; i++)
                    {
                        splitChance = 0;
                        var spaceTextParts = new List<string>();
                        for (int j = 0; j < textParts[i].Length; j++)
                        {
                            if (textParts[i][j] == ' ')
                            {
                                splitChance += settings.SpaceSplitChance;
                                if (random.Next(0, 100) < splitChance)
                                {
                                    var part = textParts[i].Substring(j + 1);
                                    spaceTextParts.Add(textParts[i].Replace(part, ""));
                                    spaceTextParts.Add(part);
                                    splitChance = 0;
                                    break;
                                }
                            }
                        }
                        if (spaceTextParts.Count > 0)
                        {
                            spaceTextParts.Reverse();
                            textParts.RemoveAt(i);
                            for (int j = 0; j < spaceTextParts.Count; j++)
                                textParts.Insert(i, spaceTextParts[j]);
                            i += spaceTextParts.Count;
                        }
                    }
                }
                else
                    textParts.Add(result.Text);
                textParts.RemoveAll(item => item.Length < 1);
                var errorIndexList = await SetMissClickErrorIndex(textParts).ConfigureAwait(false);
                var keyboardErrorCount = 0;
                for (int i = 0; i < textParts.Count; i++)
                {
                    if (textParts[i].Length > 0)
                    {
                        var textPart = new BotMessageTextPartModel();
                        textPart.Text = textParts[i];
                        textPart.Text = await ReplaceNumberToWord(textPart.Text).ConfigureAwait(false);
                        if (errorIndexList.IndexOf(i) != -1)
                        {
                            textPart = await SetMessageKeyboardError(textPart).ConfigureAwait(false);
                            if (textPart.hasMissClickError)
                                keyboardErrorCount++;
                        }
                        if(!textPart.hasMissClickError)
                            textPart = await SetCaps(textPart).ConfigureAwait(false);
                        textPart.Text = RemoveBrackets(textPart.Text);
                        textPart.Text = ConstructMessage(textPart.Text, constantText);
                        result.TextParts.Add(textPart);
                    }
                }
                if (keyboardErrorCount > 1)
                    result.hasMultiplyMissClickError = true;
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("BotWorkService", ex);
            }
            return result;
        }

        public string TextToRegex(string Text)
        {
            try
            {
                var regexstres = Text.Split("#|;|#");
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

        public async Task<string> GetApologies(BotMessageTextPartModel BotMessageTextPart)
        {
            string result = null;
            try
            {
                result = $"((Простите_Извините_Извиняюсь_Прошу прощения), " +
                               $"в (тексте_сообщении) (есть_присутствует_допущена_имеется_находится) (ошибка_опечатка_описка). " +
                               $"(Правильно_Вместо ошибки должно быть_Нужно, чтобы было): {BotMessageTextPart.BotMessageCorrectTexts}." +
                           $"_* {BotMessageTextPart.BotMessageCorrectTexts}" +
                           $"_(Простите_Извините_Извиняюсь_Прошу прощения): {BotMessageTextPart.BotMessageCorrectTexts}" +
                           $"_^ {BotMessageTextPart.BotMessageCorrectTexts})";
                result = await RandMessage(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("BotWorkService", ex);
            }
            return result;
        }

        public async Task<string> GetApologies()
        {
            string result = null;
            try
            {
                result = "(Простите_Извините_Извеняюсь_Прошу прощения), за (ошибки_опечатки_описки) (в тексте_в сообщении_)";
                result = await RandMessage(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("BotWorkService", ex);
            }
            return result;
        }

        public async Task<string> GetCapsApologies()
        {
            string result = null;
            try
            {
                result = "(Простите_Извините_Извеняюсь_Прошу прощения), (случайно_что_) капс включился";
                result = await RandMessage(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("BotWorkService", ex);
            }
            return result;
        }

        public async Task<string> InsertText(string Message, string InsertableText = "")
        {
            string result = Message;
            try
            {
                if ((InsertableText.Length > 0) && (result.IndexOf("<#ITP>") != -1))
                {
                    result = result.Replace("<#ITP>", InsertableText);
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("BotWorkService", ex);
            }
            if(result.IndexOf("<#ITP>") != -1)
                result = result.Replace("<#ITP>", "");
            return result;
        }

        public async Task<string> AudioReaction()
        {
            string result = null;
            try
            {
                result = "(Простите_Извините_Извеняюсь_Прошу прощения), (не могли бы вы_можете ли вы_не затруднит ли вас) (написать_изложить_ответить) текстом, (своё_ваше_присланное вами) (аудио_голосовое) сообщение.";
                result = await RandMessage(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("BotWorkService", ex);
            }
            return result;
        }

        public async Task<string> GetRememberMessage(int MissionNodeId, List<MissionNodeModel> MissionNodes)
        {
            var result = "";
            try
            {
                var rememberCount = 0;
                for (int i = 0; i < MissionNodes.Count; i++)
                {
                    if (MissionNodes[i].Id > MissionNodeId)
                        rememberCount++;
                }
                if (rememberCount > 0)
                {
                    result = "(Простите_Извините_Извеняюсь_Прошу прощения), (могли бы вы_можете ли вы) (ответить_дать ответ) на ";
                    if (rememberCount < 2)
                        result += "вопрос ";
                    else if (rememberCount < 5)
                        result += "вопроса ";
                    else
                        result += "вопросов ";
                    result = await RandMessage(result).ConfigureAwait(false);
                }

            }
            catch (Exception ex)
            {
                await settingsService.AddLog("BotWorkService", ex);
            }
            return result;
        }

        public async Task<string> RandMessage(string message)
        {
            try
            {
                var RandomMessageConstantTextes = new List<MessageConstantText>();
                int LastOpen = 0;
                for (int i = 0; i < message.Length; i++)
                {
                    if ((message[i] == '(') && (isTechBrackets(message, i)))
                        LastOpen = i;
                    if ((message[i] == ')') && (LastOpen != -1) && (isTechBrackets(message, i)))
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
                await settingsService.AddLog("BotWorkService", ex);
            }
            return null;
        }

        private bool isTechBrackets(string Message, int Index)
        {
            if (((Index - 1 >= 0) && (Message[Index - 1] != '\\')) || (Index == 0))
                return true;
            return false;
        }
        
        private async Task<BotMessageTextPartModel> SetMessageKeyboardError(BotMessageTextPartModel TextPart)
        {
            var result = TextPart;
            try
            {
                var words = result.Text.Split(" ");
                var settings = settingsService.GetServerSettings();
                char[][] keyboard = new char[3][]
                    {
                    new char[12] { 'й', 'ц', 'у', 'к', 'е', 'н', 'г', 'ш', 'щ', 'з', 'х', 'ъ' },
                    new char[11] { 'ф', 'ы', 'в', 'а', 'п', 'р', 'о', 'л', 'д', 'ж', 'э' },
                    new char[9] { 'я', 'ч', 'с', 'м', 'и', 'т', 'ь', 'б', 'ю' }
                    };
                var wordsIndexes = new List<int>();
                var regex = new Regex(@"\b[А-Яа-я]+\b", RegexOptions.IgnoreCase);
                var errorChance = 100;
                for (int i = 0; i < words.Length; i++)
                {
                    if (regex.IsMatch(words[i]))
                    {
                        if (random.Next(0, 100) <= errorChance)
                        {
                            if (words[i].Length < settings.MinErrorWordLength)
                            {
                                if (random.Next(0, 100) < 30)
                                    wordsIndexes.Add(i);
                            }
                            else if ((words[i].Length > settings.MinErrorWordLength) && (words[i].Length < settings.MaxErrorWordLength))
                            {
                                if (random.Next(0, 100) < 60)
                                    wordsIndexes.Add(i);
                            }
                            else
                                wordsIndexes.Add(i);
                            errorChance /= 2;
                        }
                    }
                }
                var index = wordsIndexes[random.Next(0, wordsIndexes.Count)];
                if ((index >= 0) && (regex.IsMatch(words[index])))
                {
                    var word = words[wordsIndexes[index]];
                    var newWord = "";
                    var characterIndex = random.Next(0, word.Length - 1);
                    for (int j = 0; j < keyboard.Length; j++)
                    {
                        for (int k = 0; k < keyboard[j].Length; k++)
                        {
                            if(Regex.IsMatch(word[characterIndex].ToString(), @keyboard[j][k].ToString(), RegexOptions.IgnoreCase))
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
                                if (Char.IsUpper(word[characterIndex]))
                                    sb[characterIndex] = Char.ToUpper(sb[characterIndex]);
                                newWord = sb.ToString();
                            }
                        }
                    }
                    words[wordsIndexes[index]] = newWord;
                    result.BotMessageCorrectTexts = Regex.Replace(word, "(?:[^А-яa-z0-9- ]|(?<=['\"])s)", "");
                }
                result.Text = ConstructMessage(words.ToList());
                if (result.BotMessageCorrectTexts != null)
                    result.hasMissClickError = true;
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("BotWorkService", ex);
            }
            return result;
        }

        private async Task<BotMessageTextPartModel> SetCaps(BotMessageTextPartModel TextPart)
        {
            var result = TextPart;
            try
            {
                var words = result.Text.Split(" ");
                var settingsCapsChancePerThousandWords = settingsService.GetServerSettings().CapsChancePerThousandWords;
                if (settingsCapsChancePerThousandWords < 1)
                    settingsCapsChancePerThousandWords = 1;
                var regex = new Regex(@"\b[А-Яа-я]+\b");
                if (words.Length >= 10)
                {
                    for (int i = 0; i < words.Length; i++)
                    {
                        if ((i > 0) && (i % 100 == 0) && (random.Next(0, 100) < settingsCapsChancePerThousandWords))
                        {
                            var randomIndex = random.Next(0, words.Length);
                            if (regex.IsMatch(words[randomIndex]))
                            {
                                words[randomIndex] = words[randomIndex].ToUpper();
                                TextPart.hasCaps = true;
                            }
                        }
                    }
                }
                else if (random.Next(0, 100) < words.Length / settingsCapsChancePerThousandWords)
                {
                    var randomIndex = random.Next(0, words.Length);
                    if (regex.IsMatch(words[randomIndex]))
                    {
                        words[randomIndex] = words[randomIndex].ToUpper();
                        TextPart.hasCaps = true;
                    }
                }
                result.Text = ConstructMessage(words.ToList());
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("BotWorkService", ex);
            }
            return result;
        }

        private async Task<string> ReplaceNumberToWord(string Text)
        {
            var result = Text;
            try
            {
                var textNumbers = GetConstantTexts(Text, new Regex(@"\b[0-9]{12}\b"));
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
                Text = ConstructMessage(Text, textNumbers);
                return Text;
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("BotWorkService", ex);
            }
            return result;
        }

        private List<MessageConstantText> GetConstantTexts(string Text)
        {
            var result = new List<MessageConstantText>();
            for (int i = 0; i < Text.Length; i++)
            {
                if ((Text[i] == '(') && (isTechBrackets(Text, i)))
                {
                    var constantText = "";
                    for (int j = i + 1; j < Text.Length; j++)
                    {
                        constantText += Text[j];
                        if ((Text[j] == ')') && (isTechBrackets(Text, j)))
                        {
                            constantText = constantText.Remove(constantText.Length - 1);
                            var messageConstantText = new MessageConstantText()
                            {
                                Text = constantText,
                                TextGuid = Guid.NewGuid()
                            };
                            result.Add(messageConstantText);
                            Text = Text.Remove(i, constantText.Length).Insert(i, messageConstantText.TextGuid.ToString());
                            i = -1;
                            break;
                        }
                    }
                }
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
                Text = ReplaceFirst(Text, ConstantTexts[i].Text, ConstantTexts[i].TextGuid.ToString());
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
            var regex = new Regex(@"((\[|\]|\(|\))+|s)");
            for (int i = 0; i < Text.Length; i++)
            {
                if (regex.IsMatch(Text[i].ToString()) && (isTechBrackets(Text, i)))
                {
                    Text = Text.Remove(i, 1);
                    i--;
                }
            }
            Text = Text.Replace("\\(", "(");
            Text = Text.Replace("\\)", ")");
            Text = Text.Replace("\\[", "[");
            Text = Text.Replace("\\]", "]");
            return Text;
        }

        private async Task<string> SetPhrases(string Text)
        {
            try
            {
                var settings = settingsService.GetServerSettings();
                var header = GetHeader("<#P", Text);
                while ((header != null) && (header.Length > 0))
                {
                    EnumPhraseCategory category;
                    List<EnumPhraseType> filters = new List<EnumPhraseType>();
                    var phraseCategoryString = header.Remove(0, 3);
                    phraseCategoryString = phraseCategoryString.Remove(phraseCategoryString.Length - 2);
                    switch (phraseCategoryString)
                    {
                        case "H":
                            category = EnumPhraseCategory.Hello;
                            break;
                        case "B":
                            category = EnumPhraseCategory.Bye;
                            break;
                        case "T":
                            category = EnumPhraseCategory.Thank;
                            break;
                        case "A":
                            category = EnumPhraseCategory.Apologies;
                            break;
                        case "IS":
                            category = EnumPhraseCategory.IntroductionStart;
                            break;
                        case "IM":
                            category = EnumPhraseCategory.IntroductionMiddle;
                            break;
                        case "IE":
                            category = EnumPhraseCategory.IntroductionEnd;
                            break;
                        case "CI":
                            category = EnumPhraseCategory.CanI;
                            break;
                        default:
                            category = 0;
                            break;
                    }
                    switch (header[header.Length - 2])
                    {
                        case 'S':
                            filters.Add(EnumPhraseType.Standart);
                            filters.Add(EnumPhraseType.TimeMorning);
                            filters.Add(EnumPhraseType.TimeDay);
                            filters.Add(EnumPhraseType.TimeEvening);
                            break;
                        case 'P':
                            filters.Add(EnumPhraseType.Play);
                            break;
                        case 'R':
                            filters.Add(EnumPhraseType.Respect);
                            break;
                        case 'T':
                            filters.Add(EnumPhraseType.Trade);
                            break;
                        default:
                            filters = new List<EnumPhraseType>();
                            break;
                    }
                    if ((filters.Count > 0) && (category > 0))
                    {
                        var phrases = await GetPhrasesString(category, filters).ConfigureAwait(false);
                        var newText = "";
                        if (phrases.Count > 0)
                        {
                            if (filters.IndexOf(EnumPhraseType.Standart) != -1)
                            {
                                if (random.Next(0, 100) < settings.UseDateTimeHelloPhraseChance)
                                {
                                    var currentTime = DateTime.Now.Hour;
                                    if ((currentTime >= 5) && (currentTime < 12))
                                        newText = await RandMessage(phrases[1]);
                                    if ((currentTime >= 12) && (currentTime < 18))
                                        newText = await RandMessage(phrases[2]);
                                    if ((currentTime >= 18) && (currentTime <= 23))
                                        newText = await RandMessage(phrases[3]);
                                }
                                if (newText.Length < 1)
                                    newText = await RandMessage(phrases[0]);
                            }
                            else
                                newText = await RandMessage(phrases[random.Next(0, phrases.Count)]);
                        }

                        Text = SetPhraseCase(Text, header, newText);
                    }
                    header = GetHeader("<#P", Text);
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("TextService", ex);
            }
            return Text;
        }

        private string GetHeader(string HeaderStart, string Text)
        {
            var header = "";
            var index = Text.IndexOf(HeaderStart);
            if (index > -1)
            {
                for (int i = index; i < Text.Length && Text[i] != '>'; i++)
                {
                    header += Text[i];
                }
                if (header.Length > 0)
                    return header + ">";
            }
            return null;
        }

        private async Task<List<string>> GetPhrasesString(EnumPhraseCategory Category, List<EnumPhraseType> Types)
        {
            var result = new List<string>();
            try
            {
                var phrases = phraseService.GetByCategory(Category);
                if (phrases.Count > 0)
                {
                    for (int i = 0; i < Types.Count; i++)
                    {
                        var phrasesString = "(";
                        var filteredPhrases = phrases.Where(item => item.Type == Types[i]).ToList();
                        for (int j = 0; j < filteredPhrases.Count; j++)
                        {
                            phrasesString += filteredPhrases[j].Text + "_";
                        }
                        if (phrasesString.Length > 1)
                            result.Add(phrasesString.Remove(phrasesString.Length - 1) + ")");
                        else
                            result.Add("");
                    }
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("TextService", ex);
            }
            return result;
        }

        private string SetPhraseCase(string Text, string Pattern, string Input)
        {
            var isPreviousCharAPlot = false;
            var index = Text.IndexOf(Pattern);
            var regex = new Regex(@Pattern);
            if ((index > 0) && (Text.Length > 0))
            {
                var plotRegex = new Regex(@"((\.|\!|\?)+|s)");
                var wordRegex = new Regex(@"(([А-я]|\,|[A-z])+|s)");
                for (int i = index; i > 0; i--)
                {
                    if (plotRegex.IsMatch(Text[i].ToString()))
                    {
                        isPreviousCharAPlot = true;
                        break;
                    }
                    if (wordRegex.IsMatch(Text[i].ToString()))
                        break;
                }
                if (!isPreviousCharAPlot)
                    Input = Input.ToLower();
            }
            Text = regex.Replace(Text, Input, 1);
            return Text;
        }

        private async Task<string> SetContact(string Name, EnumGender? Gender, string Text)
        {
            try
            {
                var settings = settingsService.GetServerSettings();
                if ((((Name != null) && (Name.Length > 0)) || ((Gender != null) && (Gender.Value > 0))) && (random.Next(0, 100) < settings.UseContactPhraseChance))
                {
                    var header = GetHeader("<#C", Text);
                    var regex = new Regex(header);
                    while ((header != null) && (header.Length > 0))
                    {
                        var newWord = "";
                        if ((Name != null) && (Name.Length > 0) && (random.Next(0, 100) < settings.UseNameContactChance))
                        {
                            if ((newWord.Length > 0) && (!Regex.IsMatch(newWord, "(?:[^А-я]+|(?<=['\"])s)", RegexOptions.IgnoreCase)))
                                newWord = Name.Remove(newWord.IndexOf(" "));
                        }
                        if ((Gender != null) && (newWord.Length > 0))
                        {
                            EnumPhraseCategory phraseCategory;
                            switch (Gender.Value)
                            {
                                case EnumGender.Female:
                                    phraseCategory = EnumPhraseCategory.ContactFemale;
                                    break;
                                case EnumGender.Male:
                                    phraseCategory = EnumPhraseCategory.ContactMale;
                                    break;
                                default:
                                    phraseCategory = EnumPhraseCategory.Contact;
                                    break;
                            }
                            var phraseTypes = new List<EnumPhraseType>();
                            phraseTypes.Add(EnumPhraseType.Standart);
                            var phrases = await GetPhrasesString(phraseCategory, phraseTypes).ConfigureAwait(false);
                            if (phrases.Count > 0)
                                newWord = await RandMessage(phrases[random.Next(0, phrases.Count)]);
                        }
                        Text = regex.Replace(Text, newWord, 1);
                    }
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("TextService", ex);
            }
            return Text;
        }
        
        private async Task<string> ReplaceSecondaryText(string Text)
        {
            try
            {
                var lastOpen = -1;
                for (int i = 0; i < Text.Length; i++)
                {
                    if ((Text[i] == '[') && (isTechBrackets(Text, i)))
                        lastOpen = i;
                    if ((Text[i] == ']') && (lastOpen != -1) && (isTechBrackets(Text, i)))
                    {
                        var substring = Text.Substring(lastOpen+1, i - lastOpen);
                        var parts = substring.Split("_").ToList();
                        parts = settingsService.Shuffle(parts).ToList();
                        Text = Text.Replace(substring, string.Join(" ", parts));
                        i = -1;
                    }
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("TextService", ex);
            }
            return Text;
        }

        private async Task<List<int>> SetMissClickErrorIndex(List<string> TextParts)
        {
            var result = new List<int>();
            try
            {
                var errorChancePerTenWords = settingsService.GetServerSettings().ErrorChancePerTenWords;
                var spaceCount = TextParts.Count;
                for (int i = 0; i < TextParts.Count; i++)
                {
                    spaceCount += TextParts[i].Split(' ').Length;
                    if (random.Next(0, 100) < ((spaceCount / 10) * errorChancePerTenWords))
                    {
                        result.Add(i);
                        spaceCount = 0;
                    }
                }
                var t = 0;
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("TextService", ex);
            }
            return result;
        }

        private async Task<List<MessageConstantText>> GetURLS(string message)
        {
            var result = new List<MessageConstantText>();
            while (message.IndexOf("<#PURLLinkStart>") != -1)
            {
                int urlLinkIndexStart = message.IndexOf("<#PURLLinkStart>");
                if (urlLinkIndexStart != -1)
                {
                    int urlLinkIndexEnd = message.IndexOf("<#PURLLinkEnd>");
                    if (urlLinkIndexEnd != -1)
                    {
                        string link = "";
                        link = message.Substring(urlLinkIndexStart, urlLinkIndexEnd - urlLinkIndexStart + 14);
                        message = message.Replace(link, "");
                        var linkConstantText = new MessageConstantText()
                        {
                            Text = link,
                            TextGuid = Guid.NewGuid()
                        };
                        result.Add(linkConstantText);
                    }
                }
            }
            return result;
        }

        private string ReplaceFirst(string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }

        private async Task<List<MessageConstantText>> UpdateLinks(List<MessageConstantText> Links)
        {
            for (int i = 0; i < Links.Count; i++)
            {
                Links[i].Text = Links[i].Text.Replace("<#PURLLinkStart>", "");
                Links[i].Text = Links[i].Text.Replace("<#PURLLinkEnd>", "");
            }
            return Links;
        }
    }
}
