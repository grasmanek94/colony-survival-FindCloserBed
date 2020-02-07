using Pipliz;
using System.Collections.Generic;

namespace grasmanek94.FindCloserBed
{
    public class Chunk
    {
        public const int ChunkSize = 8;

        public Vector3Int Address { get; private set; }

        // maybe somehow make sorted on Vector3Int
        public Dictionary<Vector3Int, BedState> Beds { get; private set; }

        public int UsedBeds { get; private set; }
        public int AvailableBeds { get { return Beds.Count - UsedBeds; } }

        public Chunk(Vector3Int address)
        {
            Address = address;
            Beds = new Dictionary<Vector3Int, BedState>();
            UsedBeds = 0;
        }

        public static Vector3Int GetChunkAddress(Vector3Int position)
        {
            return position / ChunkSize;
        }

        public void Add(Vector3Int position, BedState bedState)
        {
            Beds.Add(position, bedState);
        }

        public void Remove(Vector3Int position)
        {
            Beds.Remove(position);
        }

        public bool GetBedState(Vector3Int position, out BedState bedState)
        {
            return Beds.TryGetValue(position, out bedState); ;
        }

        public void SetState(Vector3Int position, bool state)
        {
            BedState bedState;
            if(!GetBedState(position, out bedState))
            {
                return;
            }

            if(bedState.State == state)
            {
                return;
            }

            bedState.State = state;

            if (state == false)
            {
                --UsedBeds;
            }
            else
            {
                ++UsedBeds;
            }
        }
    }
}
