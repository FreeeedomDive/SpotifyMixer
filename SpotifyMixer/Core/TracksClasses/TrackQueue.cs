using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SpotifyMixer.Core.TracksClasses
{
    public class TrackQueue
    {
        #region Fields

        private readonly Queue<Track> queue;
        private readonly Queue<Track> userQueue;
        private readonly Stack<Track> previousTracks;
        private readonly ObservableCollection<Track> allTracks;
        private readonly int size;
        private readonly Random random;

        #endregion

        #region Properties

        public Track CurrentTrack { get; set; }

        #endregion

        public TrackQueue(Playlist playlist, int size = 16)
        {
            random = new Random();
            this.size = size;
            previousTracks = new Stack<Track>();
            allTracks = playlist.Tracks;
            queue = GenerateQueue();
            userQueue = new Queue<Track>();
        }

        #region Methods

        private Queue<Track> GenerateQueue()
        {
            var queue = new Queue<Track>();
            for (var i = 0; i < size; i++)
            {
                InsertNextTrack(queue);
            }

            return queue;
        }

        private void InsertNextTrack(Queue<Track> queue)
        {
            var index = random.Next(0, allTracks.Count);
            var item = allTracks[index];
            queue.Enqueue(item);
        }

        public void AddUserTrack(Track track)
        {
            if (userQueue.Contains(track)) return;
            userQueue.Enqueue(track);
            track.QueuePosition = userQueue.Count;
        }

        public Track GetNextTrack()
        {
            if (userQueue.Count != 0)
            {
                var next = userQueue.Dequeue();
                next.QueuePosition = 0;
                foreach (var track in userQueue)
                {
                    track.QueuePosition--;
                }

                CurrentTrack = next;
                return next;
            }

            if (queue.Count == 0) return null;

            if (CurrentTrack != null) previousTracks.Push(CurrentTrack);
            var nextTrack = queue.Dequeue();
            CurrentTrack = nextTrack;
            if (allTracks.Count == 0) return nextTrack;
            InsertNextTrack(queue);
            return nextTrack;
        }

        public Track GetPreviousTrack()
        {
            switch (previousTracks.Count)
            {
                case 0 when CurrentTrack == null:
                    return null;
                case 0:
                    return CurrentTrack;
            }

            var lastTrack = previousTracks.Pop();
            CurrentTrack = lastTrack;
            return lastTrack;
        }

        public void SetCurrentTrack(Track track)
        {
            CurrentTrack = track;
        }

        #endregion
    }
}