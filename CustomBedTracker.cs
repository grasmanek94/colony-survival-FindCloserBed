using BlockEntities;
using BlockEntities.Implementations;
using Pipliz;
using System.Collections.Generic;

namespace grasmanek94.FindCloserBed
{
    public class CustomBedTracker
    {
        public static Dictionary<Colony, CustomBedTracker> trackers = new Dictionary<Colony, CustomBedTracker>();

        private Dictionary<Vector3Int, Chunk> chunks;

        public int Count { get { return chunks.Count; } }

        public CustomBedTracker()
        {
            chunks = new Dictionary<Vector3Int, Chunk>();
        }

        public void Add(Vector3Int position)
        {
            lock(chunks)
            {
                Vector3Int address = Chunk.GetChunkAddress(position);
                Chunk chunk;

                if(!chunks.TryGetValue(address, out chunk))
                {
                    chunk = new Chunk(address);
                    chunks.Add(address, chunk);
                }

                var gameChunk = World.GetChunk(position.ToChunk());
                BlockEntityTracker.EntityChunk entityChunk = gameChunk?.GetEntities();
                IBlockEntity blockEntity;
                BedTracker.Bed bed;
                if (entityChunk != null && entityChunk.TryGet(position.ToChunkLocal(), out blockEntity) && (bed = (blockEntity as BedTracker.Bed)) != null)
                {
                    lock (chunk)
                    {
                        chunk.Add(position, new BedState(bed, false));
                    }
                }
            }
        }

        public bool GetChunk(Vector3Int position, out Chunk chunk)
        {
            Vector3Int address = Chunk.GetChunkAddress(position);
            return chunks.TryGetValue(address, out chunk);
        }

        public void Remove(Vector3Int position)
        {
            lock (chunks)
            {
                Chunk chunk;

                if (!GetChunk(position, out chunk))
                {
                    return;
                }

                lock (chunk)
                {
                    chunk.Remove(position);
                    if(chunks.Count == 0)
                    {
                        chunks.Remove(Chunk.GetChunkAddress(position));
                    }
                }
            }
        }

        public void SetState(Vector3Int position, bool state)
        {
            lock (chunks)
            {
                Chunk chunk;

                if (!GetChunk(position, out chunk))
                {
                    return;
                }

                lock (chunk)
                {
                    chunk.SetState(position, state);
                }
            }
        }

        public bool TryGetClosestUnused(Vector3Int position, out Vector3Int bedPosition, out BedTracker.Bed bed, int boxradius)
        {
            Vector3Int address = Chunk.GetChunkAddress(position);

            bedPosition = Vector3Int.invalidPos;
            bed = null;

            int chunkRadius = (boxradius / Chunk.ChunkSize) + 1;
            int bedDistance = boxradius;

            lock (chunks)
            {
                Chunk chunk;
                if (chunks.TryGetValue(address, out chunk))
                {
                    FindClosestBed(chunk, position, ref bedPosition, ref bed, ref bedDistance);
                }

                for (int currentStep = 1; currentStep <= chunkRadius; ++currentStep)
                {
                    Vector3Int minimum = address - currentStep;
                    Vector3Int maximum = address + currentStep;

                    for (int x = minimum.x; x <= maximum.x; ++x)
                    {
                        for (int y = minimum.y; y <= maximum.y; ++y)
                        {
                            for (int z = minimum.y; z <= maximum.z; ++z)
                            {
                                if (chunks.TryGetValue(new Vector3Int(x,y,z), out chunk))
                                {
                                    FindClosestBed(chunk, position, ref bedPosition, ref bed, ref bedDistance);
                                }
                            }
                        }
                    }

                    if (bed != null)
                    {
                        return true;
                    }
                }
            }

            return bed != null;
        }

        private void FindClosestBed(Chunk chunk, Vector3Int position, ref Vector3Int bedPosition, ref BedTracker.Bed bed, ref int bedDistance)
        {
            if(chunk.AvailableBeds == 0)
            {
                return;
            }

            lock (chunk)
            {
                int resultDistance = bedDistance * bedDistance;
                foreach (var bedEntry in chunk.Beds)
                {
                    var localBed = bedEntry.Value;

                    if (localBed.State || !localBed.Bed.IsValid)
                    {
                        continue;
                    }

                    var localBedPos = bedEntry.Key;

                    int distance = (localBedPos - position).Magnitude;
                    if (distance < resultDistance)
                    {
                        resultDistance = distance;
                        bedDistance = Math.Sqrt(distance);
                        bedPosition = localBedPos;
                        bed = localBed.Bed;
                    }
                }
            }
        }
    }
}
