using Pipliz;
using System.Collections.Generic;

namespace grasmanek94.FindCloserBed
{
    public class Chunk
    {
        public const int ChunkSize = 16;

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

        public Vector3Int GetCenterPosition()
        {
            return (Address * ChunkSize) - (ChunkSize / 2);
        }

        public Vector3Int[] GetCorners()
        {
            Vector3Int center = GetCenterPosition();
            int modifier = (ChunkSize / 2);

            return new Vector3Int[]
            {
                new Vector3Int(center.x - modifier, center.y - modifier, center.z - modifier),
                new Vector3Int(center.x - modifier, center.y - modifier, center.z + modifier),
                new Vector3Int(center.x - modifier, center.y + modifier, center.z - modifier),
                new Vector3Int(center.x - modifier, center.y + modifier, center.z + modifier),
                new Vector3Int(center.x + modifier, center.y - modifier, center.z - modifier),
                new Vector3Int(center.x + modifier, center.y - modifier, center.z + modifier),
                new Vector3Int(center.x + modifier, center.y + modifier, center.z - modifier),
                new Vector3Int(center.x + modifier, center.y + modifier, center.z + modifier)
            };
        }

        public bool WithinRange(Vector3Int position, int range)
        {
            Vector3Int[] corners = GetCorners();
            int dblRange = range * range;
            foreach(Vector3Int corner in corners)
            {
                if((corner - position).Magnitude < dblRange)
                {
                    return true;
                }
            }
            return false;
        }

        public void Add(Vector3Int position, BedState bedState)
        {
            Beds.Add(position, bedState);
        }

        public void Remove(Vector3Int position)
        {
            BedState bedState;
            if (!GetBedState(position, out bedState))
            {
                return;
            }

            if(bedState.State)
            {
                --UsedBeds;
            }

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

            if (state) {
                ++UsedBeds;
            } else {
                --UsedBeds;
            }
        }
    }
}
