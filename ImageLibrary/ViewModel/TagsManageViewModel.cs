using CommunityToolkit.Mvvm.Messaging;
using ImageLibrary.Data;
using ImageLibrary.Messages;
using ImageLibrary.Services;
using Microsoft.Maui.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageLibrary.ViewModel

{
    public partial class TagsManageViewModel : BaseViewModel
    {
        TagService tagService;
        public ObservableCollection<Tag> Tags { get; } = new();

        public List<Object> SelectedTags { get; set; } = new();


        public TagsManageViewModel(TagService tagsService)
        {
            this.tagService = tagsService;
           
            Task.Run(() => GetTagsAsync());
            
        }

        public TagsManageViewModel()
        {

        }


        [ObservableProperty]
        bool isRefreshing;

        [ObservableProperty]
        string currentEntryValue;


        [ObservableProperty]
        int selectionCount;

        [RelayCommand]
        void SelectionHasChanged(Object param)//object sender, SelectionChangedEventArgs e
        {

            this.SelectionCount = SelectedTags.Count;
        }

        [RelayCommand]
        public async Task RefreshTags()
        {

            tagService.ClearTagCache();
            await GetTagsAsync();
            //Simulate longer delay
            //IsBusy = true;
            //IsRefreshing = true;
            //await Task.Delay(8000);
            //IsRefreshing = false;
            //IsBusy = false;
        }

        [RelayCommand]
        async Task GetTagsAsync()
        {
            if (IsBusy)
                return;
            try
            {

                IsBusy = true;
                var allTags = await tagService.GetTags();
                allTags = allTags.OrderBy(a => a.Name).ToList();
                if (Tags.Count != 0)
                    Tags.Clear();

                foreach (var tagItem in allTags)
                    Tags.Add(tagItem);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unable to get tags: {ex.Message}");
                await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        async Task AddTag(string tagValue)
        {
            try
            {
                if (string.IsNullOrEmpty(tagValue))
                {
                    return;
                }

                var existingMatch = (from t in this.Tags where t.Name.Equals(tagValue, StringComparison.OrdinalIgnoreCase) select t).FirstOrDefault();
                if (existingMatch is null)
                {
                    var newTag = await this.tagService.AddTag(tagValue);
                    this.Tags.Add(newTag);
                   var ordertedTags =  Tags.OrderBy(t => t.Name).ToList();
                    this.Tags.Clear();
                    foreach (var item in ordertedTags)
                    {
                        Tags.Add(item);
                    }

                    CurrentEntryValue = "";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unable to Add tag: {ex.Message}");
                await Shell.Current.DisplayAlert($"Error trying to add tag '{tagValue}'", ex.Message, "OK");
            }
            finally
            {
                currentEntryValue = String.Empty;
                IsBusy = false;
            }
        }


        [RelayCommand]
        async Task RemoveTag(string tagValue)
        {
            try
            {
                if (string.IsNullOrEmpty(tagValue))
                {
                    return;
                }

                var existingMatch = (from t in this.Tags where t.Name.Equals(tagValue, StringComparison.OrdinalIgnoreCase) select t).FirstOrDefault();
                if (existingMatch != null)
                {

                    //remove any ImageTag records first.
                   
                    var newTag = await this.tagService.DeleteTag(tagValue);
                    if (newTag !=0)
                    {
                        this.Tags.Remove(existingMatch);
                    }
                    
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unable to Add tag: {ex.Message}");
                await Shell.Current.DisplayAlert($"Error trying to add tag '{tagValue}'", ex.Message, "OK");
            }
        }

        [RelayCommand]
        async Task RemoveSelectedTags()
        {
            if (SelectedTags.Count == 0)
            {
                return;
            }

            ImagesDatabase db = await ImagesDatabase.Instance;
            int totalDeleted = 0;
            int requestedDeleteCount = this.SelectedTags.Count;
            var selectedCopy = SelectedTags.ToList();
            foreach (var obj in selectedCopy)
            {
                var thisObj = (Tag)obj;
                var recsRemovedFromDb = await db.DeleteTagItemAsync(thisObj);
                if (recsRemovedFromDb > 0)
                {
                    totalDeleted += 1;
                    
                    this.Tags.Remove(thisObj);
                    SelectedTags.Remove(thisObj);
                
                }
            }

            if (totalDeleted != requestedDeleteCount)
            {
                await Shell.Current.DisplayAlert("Could not delete all tags", "Please check your image library for images assocaited with those tags.", "OK");
            }
            else
            {
                SelectedTags.Clear();
                IsRefreshing = false;

            }

        }
    }
}
