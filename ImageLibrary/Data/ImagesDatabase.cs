//using AndroidX.Startup;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageLibrary.Data
{
    public class ImagesDatabase
    {
        static SQLiteAsyncConnection Database;

        public static  readonly AsyncLazy<ImagesDatabase> Instance =
            new AsyncLazy<ImagesDatabase>(async () =>
            {
                var instance = new ImagesDatabase();
                try
                {
                    CreateTableResult result = await Database.CreateTableAsync<ImageFileInfo>();

                    CreateTableResult res = await Database.CreateTableAsync<Tag>();

                    CreateTableResult res2 = await Database.CreateTableAsync<ImageTag>();
                }
                catch (Exception)
                {

                    throw;
                }
                return instance;
            });

        public ImagesDatabase()
        {
            Database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
        }

        #region Images
        public Task<List<ImageFileInfo>> GetImageItemsAsync(int MaxRows =0)
        {
            if (MaxRows ==0)
            {
                MaxRows = 50;
            }
          //  var tblInfo =  Database.GetTableInfoAsync("imagefiles").Result;
            return Database.Table<ImageFileInfo>().Take(MaxRows).ToListAsync();
        }

        public async Task<List<ImageFileInfo>> GetImageItemsByNameLikeAsync(string partialName)
        {
            //First get a match on file names
            var reply = new List<ImageFileInfo>();
            var sql = "SELECT * FROM [imagefiles] WHERE [Name] LIKE '%" + partialName + "%' OR [Description] LIKE '%" + partialName + "%'";
            var nameMatchRecs = await Database.QueryAsync<ImageFileInfo>(sql);//,"COLLATE NOCASE"
            //var nameMatchRecs = await Database.QueryAsync<ImageFileInfo>("SELECT * FROM [imagefiles] WHERE [Name] LIKE '%" + partialName + "'% OR [Description] LIKE '%" + partialName + "'%");
            if (nameMatchRecs?.Count > 0)
            {
                reply.AddRange(nameMatchRecs);
            }

            return reply;
        }

        public async Task<List<ImageFileInfo>> GetImageItemsByName(string fullName)
        {
            //First get a match on file names
            var reply = new List<ImageFileInfo>();
            var sql = "SELECT * FROM [imagefiles] WHERE [Name] LIKE '" + fullName + "' OR [Description] LIKE '" + fullName + "'";
            var nameMatchRecs = await Database.QueryAsync<ImageFileInfo>(sql);
            if (nameMatchRecs?.Count > 0)
            {
                reply.AddRange(nameMatchRecs);
            }

            return reply;
        }

        public Task<ImageFileInfo> GetImageItemByNameAsync(string Name)
        {
            return Database.Table<ImageFileInfo>().Where(i => i.Name == Name).FirstOrDefaultAsync();
        }

        public Task<ImageFileInfo> GetImageItemAsync(int id)
        {
            return Database.Table<ImageFileInfo>().Where(i => i.ID == id).FirstOrDefaultAsync();
        }

        public async Task< List<ImageFileInfo>> GetImageItemByTagAsync (Tag tag)
        {
            var recs =  Database.Table<ImageTag>().Where(it => it.TagId == tag.ID).ToListAsync().Result;

            List<ImageFileInfo> replyList = new List<ImageFileInfo>();
            foreach (var imageTag in recs)
            {
                var thisRec = await GetImageItemAsync(imageTag.ImageId);
                replyList.Add(thisRec); 
            }

            return await Task.Run(() => replyList);
        }


        public Task<int> GetTotalImagesCountAsync()
        {
           // return Database.Table<ImageFileInfo>().Where(i => i.ID == id).FirstOrDefaultAsync();
           return Database.Table<ImageFileInfo>().CountAsync();
        }

        public Task<int> SaveImageItemAsync(ImageFileInfo item)
        {
            if (item.ID !=0)
            {
                return Database.UpdateAsync(item);
            }
            else
            {
                return Database.InsertAsync(item);
            }
        }


        public Task<int> DeleteImageItemAsync(ImageFileInfo item)
        {
            return Database.DeleteAsync(item);
        }

        #endregion

        #region Tags
        public Task<List<Tag>> GetAllTagAsync()
        {
            //if (!Database.TableMappings.Any(m => m.MappedType.Name == typeof(Tag).Name))
            //{
            //   var res = Database.CreateTableAsync(typeof(Tag),CreateFlags.AutoIncPK).Result;
            //   // await Database.CreateTablesAsync(CreateFlags.None, typeof(TodoItem)).ConfigureAwait(false);
            //    //initialized = true;
            //}

            //var tblInfo = Database.GetTableInfoAsync("tags").Result;
            var recs = Database.Table<Tag>().ToListAsync();
            return recs;

           // return Database.Table<Tag>().ToListAsync();
        }

        public Task<Tag> GetTagItemByNameAsync(string Name)
        {
            return Database.Table<Tag>().Where(i => i.Name == Name).FirstOrDefaultAsync();
        }

        public Task<Tag> GetTagItemAsync(int id)
        {
            return Database.Table<Tag>().Where(i => i.ID == id).FirstOrDefaultAsync();
        }

        public Task<int> SaveTagItemAsync(Tag item)
        {
            if (item.ID != 0)
            {
                return Database.UpdateAsync(item);
            }
            else
            {
                return Database.InsertAsync(item);
            }
        }

        public Task<int> DeleteTagItemAsync(Tag item)
        {
            return Database.DeleteAsync(item);
        }


        public Task<List<ImageTag>> GetImageItemTagIdsAsync(int imageId)
        {
           

           var reply = Database.Table<ImageTag>().Where(i => i.ImageId == imageId).ToListAsync();


            return reply;
        }

        public Task<List<ImageTag>> GetImageTagsAllAsync()
        {


            var reply = Database.Table<ImageTag>().ToListAsync();


            return reply;
        }


        public Task<int> SaveImageTagItemAsync (ImageTag item)
        {
            if (item.ID != 0)
            {
                return Database.UpdateAsync(item);
            }
            else
            {
                return Database.InsertAsync(item);
            }
        }

        public Task<int> DeleteImageTagItemAsync(ImageTag item)
        {
            return Database.DeleteAsync(item);
        }


        #endregion

    }
}
