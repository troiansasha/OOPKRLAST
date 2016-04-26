using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Storage_of_data
{
    /// Nabir mozhlvich korustuvachiv

    public enum Character { Insert, Update, Delete };

    /// interfeis opredilyae povediku chranilishcha dannich

    interface IStorageData
    {

        /// Pidkluchaye korustuvacha do bazi


        bool Attach(IUser user);

        /// Vidkluchaye korustuvacha vid bazi

        void Detach(IUser user);

        /// Udalenie faila s doka


        bool DeleteFileFromStorage(IUser user, string NickName, string fileName);

        /// Delete document


        bool DeleteDoc(IUser user, string NickName);

        /// Obnovliaye fail v documente

        bool UpdateFileFromStorage(IUser user, string NickName, string fileName, string text);

        /// Edit new file to document or create and edit


        bool InsertFileToStorage(IUser user, string NickName, string fileName, string text);
    }

    /// Klass Sorage data

    class StorageData : IStorageData
    {

        /// List of authorised users

        List<IUser> Users;

        /// List of documents

        List<IDocument> Docs;

        /// Beckup document On name or null


        private IDocument GetDocumentByNickName(string NickName)
        {
            foreach (var doc in Docs)
            {
                if (doc.NickName.Equals(NickName))
                    return doc;
            }
            return null;
        }
        public StorageData()
        {
            Docs = new List<IDocument>();
            Users = new List<IUser>();
        }
        public bool Attach(IUser user)
        {
            if (user.IsCharacter(Character.Insert) ||
                user.IsCharacter(Character.Delete) ||
                user.IsCharacter(Character.Update))
            {
                //if we see one or more Characters- authorise
                Users.Add(user);
                return true;
            }
            return false;
        }
        public void Detach(IUser user)
        {
            if (Users.Contains(user))
                Users.Remove(user);
        }
        public bool DeleteFileFromStorage(IUser user, string NickName, string fileName)
        {
            //If character authoised and if user have roll Delet
            if (Users.Contains(user) && user.IsCharacter(Character.Delete))
            {
                IDocument doc = GetDocumentByNickName(NickName);
                if (doc != null)
                {
                    if (doc.DeleteFile(fileName))  //If deliting is complete
                    {
                        if (doc.Files.Count == 0)  //If there are no files in document
                            DeleteDoc(user, NickName);  //Delete document
                        return true;
                    }
                }
            }
            return false;
        }
        public bool DeleteDoc(IUser user, string NickName)
        {
            if (Users.Contains(user) && user.IsCharacter(Character.Delete))
            {
                IDocument doc = GetDocumentByNickName(NickName);
                if (doc != null)
                {
                    doc.Dispose();
                    Docs.Remove(doc);
                    return true;
                }
            }
            return false;
        }
        public bool InsertFileToStorage(IUser user, string NickName, string fileName, string text)
        {
            if (Users.Contains(user) && user.IsCharacter(Character.Insert))
            {
                IDocument doc = GetDocumentByNickName(NickName);
                if (doc == null)
                {
                    doc = new Document(NickName);
                    Docs.Add(doc);
                }
                else if (doc.Files.Contains(fileName))  //If document has this file
                    return false;
                doc.AddFile(fileName, text);
                return true;
            }
            return false;
        }
        public bool UpdateFileFromStorage(IUser user, string NickName, string fileName, string text)
        {
            //If character authoised and if user have roll Updat
            if (Users.Contains(user) && user.IsCharacter(Character.Update))
            {
                IDocument doc = GetDocumentByNickName(NickName);
                if (doc != null)
                {
                    if (doc.Files.Contains(fileName))
                    {
                        doc.UpdateFile(fileName, text);
                        return true;
                    }
                }
            }
            return false;
        }
    }

    /// The interface defines the behavior of a group of users

    interface IGroup
    {

        /// List of rolls

        List<Character> Characters { get; }

        /// Edit user to group


        void UserAdding(IUser user);
    }

    /// An abstract class of user groups

    abstract class Group : IGroup
    {

        ///List of rolls

        public List<Character> Characters { get; private set; }

        /// Construckter


        public Group(params Character[] roles)
        {
            Characters = roles != null ? roles.ToList() : new List<Character>();
        }
        public void UserAdding(IUser user)
        {
            user.Groups.Add(this);
        }
    }

    ///Menegers (all rolls)

    class Admins : Group
    {
        public Admins() :
            base(Character.Update, Character.Insert, Character.Delete)
        { }
    }

    /// Guests

    class Visitors : Group
    {
        public Visitors() :
            base()
        { }
    }

    /// Employees ( sample + addition)

    class Workers : Group
    {
        public Workers() :
            base(Character.Insert)
        { }
    }

    /// Interfeis detected what do user



    interface IUser
    {

        /// Change password


        bool ChangePass(string oldPassword, string newPass);

        /// Authentication



        bool Verification(string userNickName, string pass);

        /// If user have roll


        bool IsCharacter(Character character);

        /// List of grops of user

        ICollection<IGroup> Groups { get; }
    }

    ///Abstract user class

    class User : IUser
    {

        /// Name of user

        public string userNickName { get; private set; }

        /// Pass

        string pass;

        /// List of grops of user

        public ICollection<IGroup> Groups { get; private set; }

        /// Designer causes registration

        public User(string userNickName, string pass)
        {
            this.userNickName = userNickName;
            this.pass = pass;
            Groups = new List<IGroup>();
        }
        public bool ChangePass(string oldPassword, string newPass)
        {
            if (oldPassword == this.pass)
            {
                this.pass = newPass;
                return true;
            }
            return false;
        }
        public bool Verification(string userNickName, string pass)
        {
            return this.userNickName.Equals(userNickName) && this.pass.Equals(pass);
        }
        public bool IsCharacter(Character character)
        {
            foreach (var group in Groups)
                if (group.Characters.Contains(character))
                    return true;
            return false;
        }
    }

    /// Interfaith detected what do user

    interface IDocument : IDisposable
    {

        /// Id document

        int ID { get; }

        /// Name doc

        string NickName { get; }

        /// document files

        List<string> Files { get; }

        /// add file to document


        void AddFile(string fileName, string body);

        /// Update file in the document



        bool UpdateFile(string fileName, string body);

        ///Delite files from document


        bool DeleteFile(string fileName);

        /// delet all files in document

        new void Dispose();
    }
    class Document : IDocument, IDisposable
    {

        /// Indoor counter identity documents


        static int counter = 0;
        public int ID { get; private set; }
        public string NickName { get; private set; }
        public List<string> Files { get; private set; }

        /// The constructor creates a document on the correct path and sets the counter



        public Document(string name)
        {
            ID = ++counter;
            this.NickName = name;
            Files = new List<string>();
            if (!Directory.Exists(NickName))
                Directory.CreateDirectory(NickName);
        }
        public void AddFile(string fileName, string body)
        {
            var sw = new StreamWriter(MakeFileName(fileName));
            sw.WriteLine(body);
            sw.Close();
            Files.Add(fileName);
        }

        public bool UpdateFile(string fileName, string body)
        {
            if (Files.Equals(fileName))
            {
                string name = MakeFileName(fileName);
                if (!File.Exists(name))
                    return false;
                var sw = new StreamWriter(name);
                sw.WriteLine(body);
                sw.Close();
                return true;
            }
            return false;
        }

        private string MakeFileName(string fileName)
        {
            string file = NickName + "\\" + fileName + ".data";
            return file;
        }


        public bool DeleteFile(string fileName)
        {
            if (Files.Equals(fileName))
            {
                string name = MakeFileName(fileName);
                if (File.Exists(name))
                    File.Delete(name);
                return true;
            }
            return false;
        }

        public void Dispose()
        {
            foreach (var file in Files)
            {
                string name = NickName + "\\" + file;
                if (File.Exists(name))
                    File.Delete(name);
            }
            if (Directory.Exists(NickName))
                Directory.Delete(NickName);
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            IStorageData storage = new StorageData();
            IGroup managers = new Admins();
            IGroup workers = new Workers();
            IGroup guests = new Visitors();

            IUser fist_user = new User("fist_user", "qwerty");
            IUser second_user = new User("second_user", "12345");
            IUser third_user = new User("third_user", "qwerty");
            IUser fourth_user = new User("fourth_user", "qwerty");

            managers.UserAdding(fist_user);
            workers.UserAdding(second_user);
            guests.UserAdding(third_user);
            managers.UserAdding(fourth_user);

            if (storage.Attach(fist_user))
                Console.WriteLine("Attach OK");
            else
                Console.WriteLine("Attach FAIL");
            if (storage.Attach(second_user))
                Console.WriteLine("Attach OK");
            else
                Console.WriteLine("Attach FAIL");
            if (storage.Attach(third_user))    //Visitors group does not have rights

                Console.WriteLine("Attach OK");
            else
                Console.WriteLine("Attach FAIL");

            if (storage.InsertFileToStorage(fist_user, "Doc1", "file1", "Text from file1 of document1"))
                Console.WriteLine("Insert OK");
            else
                Console.WriteLine("Insert FAIL");
            if (storage.InsertFileToStorage(third_user, "Doc2", "file1", "Text from file1 of document2")) //No acces
                Console.WriteLine("Insert OK");
            else
                Console.WriteLine("Insert FAIL");
            storage.Detach(second_user);     //Rozlogin
            if (storage.InsertFileToStorage(second_user, "Doc2", "file1", "Text from file1 of document2")) //Rozloginen
                Console.WriteLine("Insert OK");
            else
                Console.WriteLine("Insert FAIL");
            storage.Attach(second_user);
            if (storage.InsertFileToStorage(second_user, "Doc2", "file1", "Text from file1 of document2")) //ОК
                Console.WriteLine("Insert OK");
            else
                Console.WriteLine("Insert FAIL");

            if (storage.DeleteDoc(second_user, "Doc2")) //No acces
                Console.WriteLine("Delete OK");
            else
                Console.WriteLine("Delete FAIL");

            if (storage.UpdateFileFromStorage(fist_user, "Doc1", "file1", "New text from fil1 of document1)")) //ОК
                Console.WriteLine("Update OK");
            else
                Console.WriteLine("Update FAIL");


            storage.Detach(fist_user);
            storage.Detach(third_user);
            Console.ReadKey();
        }
    }
}
