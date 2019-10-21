using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebMatrix.WebData;
using YourMail.Models;
using YourMail.Interfaces;
using YourMail.Infrastructure;
using System.Threading.Tasks;

namespace YourMail.Controllers
{
    public class MessageController : Controller
    {
        public ActionResult InfoFirSidePanel()
        {
            var stringList = new List<string> { "", "", "", "", "", "" };

            if (WebSecurity.IsAuthenticated && WebSecurity.Initialized)
            {
                var counter = 0;
                var userId = WebSecurity.CurrentUserId;
                using (var db = new DataBaseContext())
                {
                    var user = db.UserProfiles.First(x => x.Id == userId);
                    var a = new[] { user.IncomingLetters.Count(), user.SendLetters.Count(), user.SpamLetters.Count() };
                    for(var i = 0; i < 3; i++)
                    {
                        if (a[i] == 0)
                        {
                            stringList[counter] = "0";
                            counter++;
                            stringList[counter] = "0";
                            counter++;
                        }
                        else
                        {
                            stringList[counter] = user.listTypesOfLetter[i].Where(x => x.Subject != null && x.IsRead == false).Count().ToString();
                            counter++;
                            stringList[counter] = user.listTypesOfLetter[i].Count().ToString();
                            counter++;
                        }
                    }
                }
            }
            return PartialView("_SidePanel", stringList);
        }

        [Authorize]
        public FilePathResult DownloadFile(int? letterId)
        {
            var userId = WebSecurity.CurrentUserId;
            using (var db = new DataBaseContext())
            {
                var letter = db.LettersForDB.FirstOrDefault(x => x.Id == letterId);
                if (letter != null)
                {
                    if ((letter.SendLetters.Any(x => x.OrderUser.Id == userId)) 
                        || (letter.IncomingLetters.Any(x => x.OrderUser.Id == userId)) 
                        || (letter.SendLetters.Any(x => x.OrderUser.Id == userId)))
                    {
                        if (letter.FilePuth != null)
                        {
                            return File(letter.FilePuth, letter.FileType, letter.FileName);
                        }
                    }
                }
            }
            return null;
        }

        [Authorize]
        public async Task<ActionResult> OpenLetter(int? letterId, int? numberOfType)
        {
            var userId = WebSecurity.CurrentUserId;

            using (var db = new DataBaseContext())
            {
                if (numberOfType != null && numberOfType < 3)
                {
                    var typeOfLetter = await db.listTypesOfLetter[(int)numberOfType].FirstOrDefaultAsync(x => x.Id == letterId && x.OrderUser.Id == userId);
                    if (typeOfLetter != null)
                    {
                         typeOfLetter.IsRead = true;
                        db.Entry(typeOfLetter).State = EntityState.Modified;
                        db.SaveChanges();
                        ViewBag.NumberOfType = numberOfType;
                        return View(new Letter(typeOfLetter.LetterForDB, typeOfLetter));
                    }
                }
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> DeleteAllSelected( int[] arrayIdOfLetters, int? numberOfType, int? namberOfPeage)
        {
            var listLetters = new List<ITypesOfLetter>();

            if (arrayIdOfLetters != null)
            {
                var userId = WebSecurity.CurrentUserId;
                using (var db = new DataBaseContext())
                {
                    if (numberOfType != null && numberOfType < 3)
                    {
                        var listTypeOfLetter = await (db.listTypesOfLetter[(int)numberOfType]
                                               .Join(arrayIdOfLetters, x => x.Id, y => y, (x, y) => x)
                                               .Where(x => x.OrderUser.Id == userId))
                                               .Include(x => x.LetterForDB).ToListAsync();

                        if (listTypeOfLetter != null)
                        {
                            foreach (var typeOfLetter in listTypeOfLetter)
                            {
                                typeOfLetter.LetterForDB.NumberOfOwners--;

                                if (typeOfLetter.LetterForDB.NumberOfOwners == 0)
                                {
                                    db.Entry(typeOfLetter.LetterForDB).State = EntityState.Deleted;
                                }
                                else
                                {
                                    db.Entry(typeOfLetter.LetterForDB).State = EntityState.Modified;
                                }
                                db.Entry(typeOfLetter).State = EntityState.Deleted;
                            }
                        }
                    }
                    db.SaveChanges();
                    listLetters = await db.listTypesOfLetter[(int)numberOfType].Where(x => x.OrderUser.Id == userId).OrderByDescending(x=>x.Data).ToListAsync();
                }
            }
            return Content(CustomHelperMetods.MyGrid(listLetters, namberOfPeage.ToString()));
        }

        [Authorize]
        public async Task<ActionResult> ChangePage(int? numberOfType, int? namberOfPeage)
        {
            var listLetters = new List<ITypesOfLetter>();

            var userId = WebSecurity.CurrentUserId;

            if (numberOfType != null && numberOfType < 3)
            {
                using (var db = new DataBaseContext())
                {
                    listLetters = await db.listTypesOfLetter[(int)numberOfType].Where(x => x.OrderUser.Id == userId).OrderByDescending(x=>x.Data).ToListAsync();
                }
            }

            return Content(CustomHelperMetods.MyGrid(listLetters, namberOfPeage.ToString()));
        }

        [Authorize]
        public async Task<ActionResult> DeleteLetter(int? letterId, int? numberOfType, int? namberOfPeage)
        {
            var userId = WebSecurity.CurrentUserId;
            using (var db = new DataBaseContext())
            {
                if (numberOfType != null && numberOfType < 3)
                {
                    var typeLetter = await db.listTypesOfLetter[(int)numberOfType].FirstOrDefaultAsync(x => x.LetterForDB.Id == letterId && x.OrderUser.Id == userId);
                    if (typeLetter != null)
                    {
                        if (typeLetter.LetterForDB.NumberOfOwners == 0)
                        {
                            db.Entry(typeLetter.LetterForDB).State = EntityState.Deleted;
                        }
                        else
                        {
                            db.Entry(typeLetter.LetterForDB).State = EntityState.Modified;
                        }
                        db.Entry(typeLetter).State = EntityState.Deleted;
                    }
                }
                db.SaveChanges();
            }
            return RedirectToAction("ShowTypesLetters", "Message", new { numberOfType, namberOfPeage });
        }

        [Authorize]
        public ActionResult New_letter()
        {
            ViewBag.user = User.Identity.Name;
            return View(new Letter());
        }

        [Authorize]
        public ActionResult AnswerLetter(string fromWhom)
        {
            var newLetter = new Letter();
            newLetter.ToWhoms = fromWhom;
            return View("New_letter", newLetter);
        }
        [Authorize]
        public async Task<ActionResult> ForwardLetter(int? letterId, int? numberOfType)
        {
            if (numberOfType != null && letterId != null && numberOfType < 3 && numberOfType >= 0)
            {
                var userName = User.Identity.Name;
                ViewBag.user = userName;
                using (var db = new DataBaseContext())
                {
                    var typeOfLetter = await db.listTypesOfLetter[(int)numberOfType].FirstOrDefaultAsync(x => x.LetterForDB.Id == letterId && userName == x.OrderUser.UserMail);
                    if (typeOfLetter != null)
                    {
                        typeOfLetter.ToWhoms = "";
                        return View("New_letter", new Letter(typeOfLetter.LetterForDB, typeOfLetter));
                    }
                }
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> New_letter(Letter letter, HttpPostedFileBase upload)
        {
            if (letter != null)
            {
                using (var db = new DataBaseContext())
                {
                    var listAllRecidients = GetAllRecipients(letter);
                    var listToWhomUserProfile = await db.UserProfiles.Join(listAllRecidients, x => x.UserMail, y => y, (x, y) => x)
                                                                    .Include(x => x.IncomingLetters).Include(x => x.SendLetters)
                                                                    .Include(x => x.SpamLetters).Include(x => x.SpamMeils).ToListAsync();

                    var user = await db.UserProfiles.FirstOrDefaultAsync(x => x.UserMail == User.Identity.Name);

                    letter.FromWhom = user.UserMail;
                    letter.Data = DateTime.Now;

                    var letterForDB = CreateNewLetterForDB(letter, user, listToWhomUserProfile.Count, upload);

                    db.LettersForDB.Add(letterForDB);

                    DeleteTypeFoLetterIfCountMoreThenMAX(UserProfile.MaxSendLetters, (int)NumberOfTypes.SendLetters, db, user);
                    db.SendLetters.Add(CreateTypeOfLetter<SendLetter>(letter, user, letterForDB));

                    foreach(var toWhom in listToWhomUserProfile)
                    {
                        if (toWhom.SpamMeils.Count !=0)
                        {
                            if (toWhom.SpamMeils.Any(x => x.ToWhomMail == user.UserMail))
                            {
                                DeleteTypeFoLetterIfCountMoreThenMAX(UserProfile.MaxSpamLetters, (int)NumberOfTypes.SpamLetters, db, user);
                                db.SpamLetters.Add(CreateTypeOfLetter<SpamLetter>(letter, toWhom, letterForDB));
                            }
                        }
                        else
                        {
                            DeleteTypeFoLetterIfCountMoreThenMAX(UserProfile.MaxIncomingLetters, (int)NumberOfTypes.IncomingLetters, db, user);
                            db.IncomingLetters.Add(CreateTypeOfLetter<IncomingLetter>(letter, toWhom, letterForDB));
                        }
                    }

                    db.SaveChanges(); 
                }
            }
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public ActionResult AddSpamMail()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> AddSpamMail(SpamMeil spamMeil)
        {
            using (var db = new DataBaseContext())
            {
                var user = await db.UserProfiles.FirstOrDefaultAsync(x => x.UserMail == User.Identity.Name);

                if (user != null)
                {
                    if (user.SpamMeils.Count==UserProfile.MaxSpamMail)
                    {
                        db.Entry(user.SpamMeils.Where(x=>x.Id == user.SpamMeils.Min(y => y.Id))).State = EntityState.Deleted;
                    }
                    var exist = false;
                    foreach (var mail in user.SpamMeils)
                    {
                        exist = string.Equals(mail, spamMeil.ToWhomMail) ? true : false;
                    }
                    if (!exist)
                    {
                        spamMeil.OrderUser = user;

                        db.Entry(spamMeil).State = EntityState.Added;

                        db.SaveChanges();
                    }
                }
            }
            return RedirectToAction("AddSpamMail", "Message");
        }

        [Authorize]
        public async Task<ActionResult> ShowTypesLetters(int? numberOfType, int? namberOfPeage)
        {
            var currentUserId = WebSecurity.CurrentUserId;
            var listLetters = new List<ITypesOfLetter>();
            if (numberOfType != null)
            {
                using (var db = new DataBaseContext())
                {
                    if (numberOfType != null && numberOfType < 3 && numberOfType >= 0)
                    {
                        listLetters = await db.listTypesOfLetter[(int)numberOfType].Where(x => x.OrderUser.Id == currentUserId).OrderByDescending(x => x.Data).ToListAsync();

                        ViewBag.Title = numberOfType == (int)NumberOfTypes.IncomingLetters ? "Incoming Letters" : numberOfType ==
                                                               (int)NumberOfTypes.SendLetters ? "Send Letters" : "Spam letters";
                    }
                }
            }
            if (listLetters.Count >= 1)
            {
                ViewBag.NumberOfType = numberOfType;
                ViewBag.NamberOfPeage = namberOfPeage == null ? 1 : namberOfPeage;
                return View(listLetters);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public async void DeleteTypeFoLetterIfCountMoreThenMAX(int MAXCoun, int numberOfType, DataBaseContext db, UserProfile user)
        {
            if ( db.listTypesOfLetter[numberOfType].Count(x => x.OrderUser.Id == user.Id) == MAXCoun)
            {
                var someTypeOfLetter = await db.listTypesOfLetter[numberOfType].FirstAsync(x => x.OrderUser.Id == user.Id && x.Data == db.listTypesOfLetter[numberOfType].Min(y => y.Data));

                db.Entry(someTypeOfLetter).State = EntityState.Deleted;
            }
        }

        public T CreateTypeOfLetter<T>(Letter letter, UserProfile user, LetterForDB letterForDB) where T : ITypesOfLetter, new()
        {
            var typeOfLetter = new T();
            typeOfLetter.LetterForDB = letterForDB;
            typeOfLetter.OrderUser = user;
            typeOfLetter.Subject = letter.Subject;
            typeOfLetter.Data = letter.Data;
            typeOfLetter.FromWhom = letter.FromWhom;
            typeOfLetter.ToWhoms = letter.ToWhoms;
            return typeOfLetter;
        }

        public LetterForDB CreateNewLetterForDB(Letter letter, UserProfile user, int allRecipientsCount, HttpPostedFileBase upload)
        {
            if (upload != null)
            {
                var filePuth = "~/Files/" + Guid.NewGuid().ToString();
                try
                {
                    upload.SaveAs(Server.MapPath(filePuth));
                }
                catch (Exception)
                {

                    return null;
                }

                letter.FilePuth = filePuth;
                letter.FileType = upload.ContentType;
                letter.FileName = upload.FileName;
            }
            letter.NumberOfOwners = allRecipientsCount + 1; //"+1" This is the user who sent the lette

            return (LetterForDB)letter;
        }

        public List<string> GetAllRecipients(Letter letter)
        {
            var arrayRecipientPerson = letter.ToWhoms.Split(new Char[] { ' ', ',', ';', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            return arrayRecipientPerson.Distinct().ToList(); 
        }
    }
}