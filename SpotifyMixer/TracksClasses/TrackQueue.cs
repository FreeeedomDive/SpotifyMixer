using System;
using System.Collections.Generic;

namespace SpotifyMixer.TracksClasses
{
    public class TrackQueue
    {
        private readonly Queue<Track> _queue;
        private readonly Queue<Track> _userQueue;
        public Track CurrentTrack;
        private readonly Stack<Track> _previousTracks;
        private readonly List<Track> _allTracks;
        private readonly int _size;
        private readonly Random _random;

        public TrackQueue(Playlist playlist, int size = 16)
        {
            _random = new Random();
            _size = size;
            _previousTracks = new Stack<Track>();
            _allTracks = playlist.Tracks;
            _queue = GenerateQueue();
            _userQueue = new Queue<Track>();
        }
        
        private Queue<Track> GenerateQueue()
        {
            var queue = new Queue<Track>();
            for (var i = 0; i < _size; i++)
            {
                InsertNextTrack(queue);
            }

            return queue;
        }

        private void InsertNextTrack(Queue<Track> queue)
        {
            var index = _random.Next(0, _allTracks.Count);
            var item = _allTracks[index];
            queue.Enqueue(item);
        }

        public void AddUserTrack(Track track)
        {
            if (_userQueue.Contains(track)) return;
            _userQueue.Enqueue(track);
            track.QueuePosition = _userQueue.Count;
        }

        public Track GetNextTrack()
        {
            if (_userQueue.Count != 0)
            {
                var next = _userQueue.Dequeue();
                next.QueuePosition = 0;
                foreach (var track in _userQueue)
                {
                    track.QueuePosition--;
                }
                CurrentTrack = next;
                return next;
            }
            
            if (_queue.Count == 0) return null;
            
            if (CurrentTrack != null) _previousTracks.Push(CurrentTrack);
            var nextTrack = _queue.Dequeue();
            CurrentTrack = nextTrack;
            if (_allTracks.Count == 0) return nextTrack;
            InsertNextTrack(_queue);
            return nextTrack;
        }

        public Track GetPreviousTrack()
        {
            switch (_previousTracks.Count)
            {
                case 0 when CurrentTrack == null:
                    return null;
                case 0:
                    return CurrentTrack;
            }
            var lastTrack = _previousTracks.Pop();
            CurrentTrack = lastTrack;
            return lastTrack;
        }

        public void SetCurrentTrack(Track track)
        {
            CurrentTrack = track;
        }
    }
}