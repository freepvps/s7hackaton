using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using airbot.Data;

namespace airbot.TextGeneration
{
    public static class Messages
    {
        static Random random = new Random();

        static string[] HelloArray =
        {
            "Привет!",
            "Здравствуй"
        };
        static string[] AskQuestionArray =
        {
            "Вы можете задать мне любой вопрос",
            "Спросите что-нибудь у меня:)"
        };
        static string[] ErrorSorryArray =
        {
            "Я не могу понять, о чем идет речь:(",
            "Я всего-лишь машина, имитация жизни:("
        };
        static string[] СongratulationsArray =
        {
            "Поздравляем",
            "Успех"
        };
        static string[] OkArray =
        {
            "ОК",
            "Хорошо"
        };

        public const string AskForm = "Пожалуйста, заполните анкету, это не займет много времени:)";
        public const string FormComplete = "Форма заполнена:)";
        public const string AskName = "Укажите ваше имя";
        public const string AskSurname = "Укажите вашу фамилию";
        public const string AskPassport = "Укажите свои паспортные данные";
        public const string AskCity = "В каком городе вы сейчас находитесь?";


        public const string TransferAsk = "Вам нужен трансфер в аэропорт или из него?";
        public static readonly string[] Transfering =
        {
            "Из и в аэропорт", "Из аэропорта", "В аэропорт", "Нет, трансфер не требуется"
        };
        public static string TransferResult(DateTime dateTime, string location)
        {
            return $"В {dateTime.HumanLike()} такси будет ждать вас <b>{location}</b>";
        }
        public static string TransferResult(bool inT, bool outT, Ticket ticket)
        {
            var sb = new StringBuilder();
            if (inT)
            {
                sb.AppendLine(TransferResult(ticket.Flight.StartTime.AddHours(-2), "по указанному в профиле адресу"));
            }
            if (outT)
            {
                sb.AppendLine(TransferResult(ticket.Flight.EndTime, "возле аэропорта"));
            }
            sb.AppendLine("Удачного вам полета!:)");
            return sb.ToString();
        }


        public const string NothingFlightFound = "По вашему запросу рейсов не найдено";
        public const string Help = "Привет! Задавай мне любые вопросы, я с радостью отвечу на них:)";

        public static readonly string[] YesNoPair = { "Да", "Нет" };

        public static string CityComplete(string city) => "Ваш город: " + CityName(city);
        public static string CityFail() => "Неизвестный город:(";

        public static string IntEndings(int value, string t1, string t234, string t567890)
        {
            if (value % 10 == 1) return value + " " + t1;
            if (value % 10 >= 2 && value <= 4) return value + " " + t234;
            return value + " " + t567890;
        }
        public static string HumanLike(this Flight flight)
        {
            var cityFrom = CityName(flight.From);
            var cityTo = CityName(flight.To);
            var cost = Rubles(flight.Cost);
            var start = flight.StartTime.HumanLike();
            var period = UserTimeParser.SecondsToString((int)(flight.EndTime - flight.StartTime).TotalSeconds);

            return $"<b>{cityFrom}</b> - <b>{cityTo}</b> вылет {start} продолжительность {period}, цена {cost}";
        }
        public static string Rubles(int value)
        {
            var t = value / 1000;
            if (t == 0)
            {
                return IntEndings(t, "рубль", "рубля", "рублей");
            }
            return IntEndings(t, "тысяча", "тысячи", "тысяч") + " " + IntEndings(value % 1000, "рубль", "рубля", "рублей");
        }

        public static string AskBuy(Flight flight)
        {
            return "Вы хотите купить билет на следующий рейс?" + Environment.NewLine + flight.HumanLike();
        }
        public static string BuyUnknown()
        {
            return "Я не понимаю вашего ответа:(";
        }
        public static string BuyComplete(Flight flight, Ticket ticket)
        {
            return
                "Вы купили билет на следующий рейс: " + 
                flight.HumanLike() + Environment.NewLine + 
                "Вы будете автоматически подписаны на уведомления о любых изменениях данного рейса" + 
                Environment.NewLine + "Номер вашего билета: <b>" + ticket.TicketId + "</b>";
        }
        public static string BuyFail(Flight flight)
        {
            return "Как вам угодно:)";
        }

        public static string OK() => RandomStr(OkArray);
        public static string Hello() => RandomStr(HelloArray);
        public static string Сongratulations() => RandomStr(СongratulationsArray);
        public static string AskQuestion() => RandomStr(AskQuestionArray);
        public static string ErrorSorry() => RandomStr(ErrorSorryArray);
        public static string ToArrayString(this IEnumerable<MachineLang.LangToken> tokens, string endJoin = "и") => ArrayString(tokens.Select(x => x.Value), endJoin);
        public static string Subscribe(IEnumerable<string> flights)
        {
            if (flights.Count() == 1)
            {
                return Сongratulations() + ", Вы успешно подписались на информацию о рейсе " + flights.First();
            }
            if (flights.Count() == 0)
            {
                return "Введите номер рейса на информацию о котором вы хотите подписаться";
            }
            return Сongratulations() + ", Вы успешно подписались на информацию о рейсах " + ArrayString(flights);
        }
        public static string HumanLike(this DateTime dt)
        {
            return dt.ToString("H:mm dd MMMM yyyy г.", new System.Globalization.CultureInfo("ru-RU"));
        }
        public static string NameReask(string name)
        {
            return $"Вас зовут {name}? Если нет, то укажите, пожалуйста, точное имя";
        }

        public static string CityName(string cityCode)
        {
            if (cityCode == "msk") return "Москва";
            else if (cityCode == "prm") return "Пермь";
            else if (cityCode == "spb") return "Санкт-Петербург";
            return "Мордор";
        }

        public static string ArrayString<T>(IEnumerable<T> values, string endJoin = "и")
        {
            var r = new TextList<T>(endJoin);
            r.AddRange(values);
            return r.ToString();
        }

        public static string AboardResult(IEnumerable<string> positive, IEnumerable<string> negative)
        {
            if (positive.Count() == 0)
            {
                if (negative.Count() == 0)
                {
                    return "Думаю, можно, но это не точно";
                }
                return ArrayString(negative).UpFirst() + " нельзя брать на борт";
            }
            if (negative.Count() == 0)
            {
                return ArrayString(positive).UpFirst() + " можно брать на борт";
            }
            return ArrayString(positive).UpFirst() + " можно брать на борт, но " + ArrayString(negative) + " нельзя";
        }

        public static string RandomStr(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                return string.Empty;
            }
            return args[random.Next(args.Length)];
        }

        public static string UpFirst(this string msg)
        {
            if (msg.Length == 0) return string.Empty;
            return msg.Substring(0, 1).ToUpper() + msg.Substring(1);
        }
    }
}
