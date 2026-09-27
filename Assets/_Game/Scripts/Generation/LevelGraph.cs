using System.Collections.Generic;
using UnobservedRooms.Data;

namespace UnobservedRooms.Generation
{
    public sealed class LevelGraph
    {
        public readonly Dictionary<string, RoomDefinition> Rooms = new();
        public readonly Dictionary<string, List<EdgeDefinition>> EdgesByRoom = new();

        public LevelGraph(LevelDefinition level)
        {
            foreach (var room in level.Rooms)
            { Rooms.Add(room.Id, room); EdgesByRoom.Add(room.Id, new List<EdgeDefinition>()); }
            foreach (var edge in level.Edges)
            { EdgesByRoom[edge.FromRoomId].Add(edge); EdgesByRoom[edge.ToRoomId].Add(edge); }
        }

        public string Other(EdgeDefinition edge, string roomId) => edge.FromRoomId == roomId ? edge.ToRoomId : edge.FromRoomId;
    }
}
