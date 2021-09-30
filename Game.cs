using System;
using System.Collections.Generic;

namespace ScreenScraper {
    class Game {
        
        public string Title { get; private set; }
        public string Link { get; private set; }
        public string LastUpdated { get; private set; }
        private List<string> tagsList;
        public string Tags { get; private set; }
        public Game(string title, string link, string lastUpdated, List<string> tags) {
            this.Title = title;
            this.Link = link;
            this.LastUpdated = lastUpdated;
            this.tagsList = tags;
            this.Tags = GetTags(); 
           
        }

        public string GetTags() {
            string retVal = "";
            foreach (string tag in tagsList) {
                retVal += tag + " ";
            }
            return retVal;
        }
    }
}
