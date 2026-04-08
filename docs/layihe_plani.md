# PromptGen.Api - Layih? Plan? v? ?? Ax?n? (Documentation)

Bu s?n?d **PromptGen.Api** (AI ?sasl? Telegram Bot v? API) layih?sinin ?sas i?l?m? strukturunu, x?talar? aradan qald?rmaq, botu tam ideal s?viyy?d? i?l?tm?k v? onu inki?af etdirm?k üçün laz?m olan ard?c?l add?mlar? aç?qlay?r.

## 1. Layih?nin Arxitekturas?
Layih? .NET 10 üz?rind? qurulub. T?rkibind? bunlar var:
- **Serilog:** T?tbiq daxili loglar? (fayl v? console) idar? edir.
- **Entity Framework Core (PostgreSQL):** ?stifad?çil?rin m?lumatlar?n? veril?nl?r bazas?nda saxlamaq üçün.
- **Telegram.Bot:** ?stifad?çil?rd?n g?l?n mesajlar? (polling il?) dinl?y?n kitabxana (`BotBackgroundService`).
- **Google.Gemini.AI (`Mscc.GenerativeAI`):** ??kil v? ya m?tnd?n detall? b?dii promptlar ç?xarmaq üçün API servisi.

---

## 2. Bot niy? mesajlar? oxuyurdu amma cavab vermirdi (Planla?d?rma S?hvi)?

Mövvcud `BotBackgroundService` v? `GeminiService` kodunda a?a??dak? struktur m?ntiqi s?hvl?ri var idi ki, bot t?r?find? u?ursuzluq v? ya `AI haz?rda cavab ver? bilmir` x?talar? yaran?rd?:

1. **??kill?ri Düzgün Yükl?m?m?k:** Siz Telegramdan ??kli yükl?y?nd? b?z?n yadda?dan oxuma zaman? pointer tam sona q?d?r getdiyi üçün `Convert.ToBase64` bo? m?tni çevirirdi. Orada `ms.Position = 0` v? ya `ToArray()` istifad?si h?ssas yana?ma t?l?b edirdi.
2. **IPart çevirm? u?ursuzlu?u:** Gemini API sizd?n ??kli (image) `InlineData` format?nda t?l?b edir v? siyah? mütl?q `List<IPart>` olmal?d?r. Bu struktur f?rqlilikl?ri API x?tas?na g?tirib ç?xar?rd?.
3. **Log x?tas?:** `BotBackgroundService`-d? `ILogger` obyekti yarad?lsa da, ona konstruktordan `Dependency Injection` vasit?sil? m?lumat verilm?mi?di (NullReferenceException riski).

---

## 3. Botun ?deal v?ziyy?td? i?l?m?si üçün T?l?b Olunan Add?mlar

Bu s?n?dd?ki m?ntiq t?tbiq edildikd?n sonra bot bu qaydada davranmal?d?r:

1. **?stifad?çi `/start` mesaj? at?r:** 
   - Bot ona xo? g?ldin mesaj? verir. Baza `(AppUser)` c?dv?lin? ?lav? edilir.
  
2. **K?nar logging prosesl?ri azalacaq:** 
   - `.MinimumLevel.Information()` say?sind? `HttpClient` t?mizl?nm? dövriyy?si (cleanup cycle) art?q terminala saniy?d? bir d?f? dü?m?y?c?k.

3. **?stifad?çi m?tn (Mövzu) yazd?qda (M?s: `Q?dim Qala`):**
   - Bot, "Mövzu ?sas?nda prompt haz?rlay?ram..." mesaj?n? gönd?rir.
   - `GeminiService.GeneratePromptAsync` metodu ça?r?l?r.
   - AI servisin? daxil olaraq m?tn i?l?nir, n?tic? markdown format?nda bota geri ötürülür.

4. **?stifad?çi ??kil gönd?rdikd? (`Message.Photo` mövcuddursa):**
   - Bot, ??kli "GetFile" v? "DownloadFileAsync" vasit?si il? `MemoryStream`-? yükl?yir.
   - `ToBase64String` il? Base64-? çevrilir.
   - `GeminiService.GeneratePromptFromImageAsync`-a h?m ??kil (base64) h?m d? (?g?r yaz?l?bsa) m?tn `userPrompt` gönd?rilir.
   - AI h?m ??kili h?m d? m?tni n?z?r? alaraq ?deal promptu formala?d?r?r.

---

## 4. T?hlük?sizlik v? ?lav? T?nziml?m?l?r (Recommendations)

A?a??dak? qaydalar? n?z?r? alsan?z proqram 10 d?f? daha problemsiz (stable) i?l?y?c?k:
- **`appsettings.json` T?hlük?sizliyi:** Diqq?tli olun ki, `appsettings.json` daxilind?ki `Telegram Token` v? `Gemini API Key` Git-? (GitHub-a) gönd?rilm?sin. Layih?d? mutleq `.gitignore` t?yin edib, o fayllar? gizl?din.
- **Aç?q Ba?lant?lar (Ghost Processes):** ?nki?af (Development) müdd?tind? heç vaxt layih?ni `Ctrl + C` il? dayand?rmam?? ard-arda `dotnet run` etm?yin. Bunun n?tic?si `Conflict` v? "address already in use" olacaq.
- **Log Monitorinq:** Ciddi (q?rm?z?) x?talar `logs/bot_log.txt` daxilind? saxlan?l?r. H?ft?lik olaraq onu oxuma??n?z tövsiy? edilir.

---
**T?rtibat:** AI Asistant 
**Layih? H?d?fi:** .NET 10