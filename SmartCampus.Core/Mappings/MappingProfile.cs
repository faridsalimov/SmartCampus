using AutoMapper;

using SmartCampus.Core.DTOs;
using SmartCampus.Data.Entities;

namespace SmartCampus.Core.Mappings
{



    public class MappingProfile : Profile
    {
        public MappingProfile()
        {

            CreateMap<Student, StudentDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.ApplicationUser!.FullName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.ApplicationUser!.Email))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.ApplicationUser!.PhoneNumber))
                .ForMember(dest => dest.GroupName, opt => opt.MapFrom(src => src.Group!.Name))
                .ReverseMap()
                .ForMember(dest => dest.ApplicationUser, opt => opt.Ignore())
                .ForMember(dest => dest.Group, opt => opt.Ignore());


            CreateMap<Teacher, TeacherDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.ApplicationUser!.FullName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.ApplicationUser!.Email))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.ApplicationUser!.PhoneNumber))
                .ReverseMap()
                .ForMember(dest => dest.ApplicationUser, opt => opt.Ignore());


            CreateMap<Course, CourseDto>()
                .ForMember(dest => dest.TeacherName, opt => opt.MapFrom(src => src.Teacher!.ApplicationUser!.FullName))
                .ForMember(dest => dest.GroupName, opt => opt.MapFrom(src => src.Group!.Name))
                .ReverseMap()
                .ForMember(dest => dest.Teacher, opt => opt.Ignore())
                .ForMember(dest => dest.Group, opt => opt.Ignore());


            CreateMap<Lesson, LessonDto>()
                .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Group!.Name))
                .ForMember(dest => dest.TeacherName, opt => opt.MapFrom(src => src.Teacher!.ApplicationUser!.FullName))
                .ReverseMap()
                .ForMember(dest => dest.Group, opt => opt.Ignore())
                .ForMember(dest => dest.Teacher, opt => opt.Ignore());


            CreateMap<Homework, HomeworkDto>()
                .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Group!.Name))
                .ForMember(dest => dest.TeacherName, opt => opt.MapFrom(src => src.Teacher!.ApplicationUser!.FullName))
                .ReverseMap()
                .ForMember(dest => dest.Group, opt => opt.Ignore())
                .ForMember(dest => dest.Teacher, opt => opt.Ignore());


            CreateMap<HomeworkSubmission, HomeworkSubmissionDto>()
                .ForMember(dest => dest.HomeworkTitle, opt => opt.MapFrom(src => src.Homework!.Title))
                .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => src.Student!.ApplicationUser!.FullName))
                .ReverseMap()
                .ForMember(dest => dest.Homework, opt => opt.Ignore())
                .ForMember(dest => dest.Student, opt => opt.Ignore());


            CreateMap<Grade, GradeDto>()
                .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => src.Student != null && src.Student.ApplicationUser != null ? src.Student.ApplicationUser.FullName : "Unknown"))
                .ForMember(dest => dest.GroupId, opt => opt.MapFrom(src => src.GroupId))
                .ReverseMap()
                .ForMember(dest => dest.Student, opt => opt.Ignore())
                .ForMember(dest => dest.Course, opt => opt.Ignore())
                .ForMember(dest => dest.Group, opt => opt.Ignore())
                .ForMember(dest => dest.HomeworkSubmission, opt => opt.Ignore())
                .ForMember(dest => dest.GradedByTeacher, opt => opt.Ignore())
                .ForMember(dest => dest.CourseId, opt => opt.Condition(src => src.CourseId.HasValue && src.CourseId != Guid.Empty));


            CreateMap<AttendanceRecord, AttendanceDto>()
                .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => src.Student!.ApplicationUser!.FullName))
                .ForMember(dest => dest.LessonTitle, opt => opt.MapFrom(src => src.Lesson!.Title))
                .ReverseMap()
                .ForMember(dest => dest.Student, opt => opt.Ignore())
                .ForMember(dest => dest.Lesson, opt => opt.Ignore());


            CreateMap<Announcement, AnnouncementDto>()
                .ForMember(dest => dest.TeacherName, opt => opt.MapFrom(src => src.Teacher!.ApplicationUser!.FullName))
                .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Course!.Title))
                .ReverseMap()
                .ForMember(dest => dest.Teacher, opt => opt.Ignore())
                .ForMember(dest => dest.Course, opt => opt.Ignore());


            CreateMap<Message, MessageDto>()
                .ForMember(dest => dest.SenderName, opt => opt.MapFrom(src => src.Sender!.FullName))
                .ForMember(dest => dest.ReceiverName, opt => opt.MapFrom(src => src.Receiver!.FullName))
                .ReverseMap()
                .ForMember(dest => dest.Sender, opt => opt.Ignore())
                .ForMember(dest => dest.Receiver, opt => opt.Ignore());


            CreateMap<Group, GroupDto>()
                .ForMember(dest => dest.GroupName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.GroupCode, opt => opt.MapFrom(src => src.Code))
                .ReverseMap()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.GroupName))
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.GroupCode));


            CreateMap<Schedule, ScheduleDto>()
                .ForMember(dest => dest.TeacherName, opt => opt.MapFrom(src => src.Teacher != null && src.Teacher.ApplicationUser != null ? src.Teacher.ApplicationUser.FullName : "Unknown"))
                .ForMember(dest => dest.LessonTitle, opt => opt.MapFrom(src => src.Lesson != null ? src.Lesson.Title : ""))
                .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Course != null ? src.Course.Title : ""))
                .ForMember(dest => dest.GroupName, opt => opt.MapFrom(src => src.Group != null ? src.Group.Name : ""))
                .ForMember(dest => dest.GroupId, opt => opt.MapFrom(src => src.GroupId))
                .ReverseMap()
                .ForMember(dest => dest.Teacher, opt => opt.Ignore())
                .ForMember(dest => dest.Lesson, opt => opt.Ignore())
                .ForMember(dest => dest.Course, opt => opt.Ignore())
                .ForMember(dest => dest.Group, opt => opt.Ignore());
        }
    }
}