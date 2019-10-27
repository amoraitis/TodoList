using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using TodoList.API.Models;
using TodoList.Core.Models;

namespace TodoList.API.MapperProfiles
{
    public class TodoItemProfile : Profile
    {
        public TodoItemProfile()
        {
            CreateMap<TodoItemDto, TodoItem>();
        }
    }
}
