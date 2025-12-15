using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using SmartCampus.Data.Context;
using SmartCampus.Data.Entities;

namespace SmartCampus.Data.Seeders
{



    public class DataSeeder
    {
        private readonly SmartCampusDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DataSeeder(SmartCampusDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }




        public async Task SeedAllDataAsync()
        {
            try
            {

                await SeedRolesAsync();


                await SeedUsersAsync();


                await SeedGroupsAsync();
                await _context.SaveChangesAsync();


                await SeedStudentsAsync();
                await _context.SaveChangesAsync();


                await SeedCoursesAsync();
                await _context.SaveChangesAsync();


                await SeedLessonsAsync();
                await _context.SaveChangesAsync();


                await SeedSchedulesAsync();
                await _context.SaveChangesAsync();


                await SeedHomeworkAsync();
                await _context.SaveChangesAsync();


                await SeedAnnouncementsAsync();
                await _context.SaveChangesAsync();


                await SeedAttendanceRecordsAsync();
                await _context.SaveChangesAsync();


                await SeedGradesAsync();
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error seeding database", ex);
            }
        }




        private async Task SeedRolesAsync()
        {
            var roles = new[] { "Admin", "Teacher", "Student" };
            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }




        private async Task SeedUsersAsync()
        {

            var adminExists = await _userManager.FindByEmailAsync("admin@smartcampus.com");
            if (adminExists == null)
            {
                var adminUser = new ApplicationUser
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = "admin",
                    Email = "admin@smartcampus.com",
                    EmailConfirmed = true,
                    FullName = "System Administrator",
                    UserRole = "Admin",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                var result = await _userManager.CreateAsync(adminUser, "Admin@123456");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }


            var teacherEmails = new[]
            {
                ("james.smith@smartcampus.com", "James Smith", "T001", "Computer Science", "Software Engineering"),
                ("robert.johnson@smartcampus.com", "Robert Johnson", "T002", "Mathematics", "Calculus & Linear Algebra"),
                ("sarah.williams@smartcampus.com", "Sarah Williams", "T003", "English", "English Literature"),
                ("michael.brown@smartcampus.com", "Michael Brown", "T004", "Physics", "Modern Physics"),
                ("emma.davis@smartcampus.com", "Emma Davis", "T005", "Chemistry", "Organic Chemistry"),
                ("david.wilson@smartcampus.com", "David Wilson", "T006", "History", "World History"),
                ("jessica.taylor@smartcampus.com", "Jessica Taylor", "T007", "Art", "Contemporary Art"),
                ("christopher.anderson@smartcampus.com", "Christopher Anderson", "T008", "Physical Education", "Sports Science"),
            };

            var teachersToAdd = new List<Teacher>();

            foreach (var (email, fullName, teacherId, department, specialization) in teacherEmails)
            {
                var teacherIdExists = await _context.Teachers.AnyAsync(t => t.TeacherId == teacherId);
                if (teacherIdExists)
                    continue;

                var teacherUser = await _userManager.FindByEmailAsync(email);
                if (teacherUser == null)
                {
                    var teacher = new ApplicationUser
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserName = email.Split()[0],
                        Email = email,
                        EmailConfirmed = true,
                        FullName = fullName,
                        UserRole = "Teacher",
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    };

                    var result = await _userManager.CreateAsync(teacher, "Teacher@123456");
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(teacher, "Teacher");

                        var teacherEntity = new Teacher
                        {
                            Id = Guid.NewGuid(),
                            TeacherId = teacherId,
                            ApplicationUserId = teacher.Id,
                            Department = department,
                            Specialization = specialization,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        };
                        teachersToAdd.Add(teacherEntity);
                    }
                }
                else
                {
                    var teacherEntityExists = await _context.Teachers.AnyAsync(t => t.ApplicationUserId == teacherUser.Id);
                    if (!teacherEntityExists)
                    {
                        var teacherEntity = new Teacher
                        {
                            Id = Guid.NewGuid(),
                            TeacherId = teacherId,
                            ApplicationUserId = teacherUser.Id,
                            Department = department,
                            Specialization = specialization,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        };
                        teachersToAdd.Add(teacherEntity);
                    }
                }
            }

            if (teachersToAdd.Count > 0)
            {
                _context.Teachers.AddRange(teachersToAdd);
                await _context.SaveChangesAsync();
            }


            var studentEmails = new[]
            {
                ("john.davis@smartcampus.com", "John Davis", "S001"),
                ("emma.miller@smartcampus.com", "Emma Miller", "S002"),
                ("oliver.wilson@smartcampus.com", "Oliver Wilson", "S003"),
                ("sophia.moore@smartcampus.com", "Sophia Moore", "S004"),
                ("liam.taylor@smartcampus.com", "Liam Taylor", "S005"),
                ("ava.anderson@smartcampus.com", "Ava Anderson", "S006"),
                ("noah.thomas@smartcampus.com", "Noah Thomas", "S007"),
                ("isabella.jackson@smartcampus.com", "Isabella Jackson", "S008"),
                ("lucas.white@smartcampus.com", "Lucas White", "S009"),
                ("mia.harris@smartcampus.com", "Mia Harris", "S010"),
                ("mason.martin@smartcampus.com", "Mason Martin", "S011"),
                ("charlotte.garcia@smartcampus.com", "Charlotte Garcia", "S012"),
                ("ethan.rodriguez@smartcampus.com", "Ethan Rodriguez", "S013"),
                ("amelia.lee@smartcampus.com", "Amelia Lee", "S014"),
                ("logan.perez@smartcampus.com", "Logan Perez", "S015"),
                ("harper.thompson@smartcampus.com", "Harper Thompson", "S016"),
            };

            foreach (var (email, fullName, studentId) in studentEmails)
            {
                var studentUser = await _userManager.FindByEmailAsync(email);
                if (studentUser == null)
                {
                    var student = new ApplicationUser
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserName = email.Split()[0],
                        Email = email,
                        EmailConfirmed = true,
                        FullName = fullName,
                        UserRole = "Student",
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    };

                    var result = await _userManager.CreateAsync(student, "Student@123456");
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(student, "Student");
                    }
                }
            }
        }




        private async Task SeedGroupsAsync()
        {
            if (await _context.Groups.AnyAsync())
                return;

            var groups = new[]
            {
                new Group { Id = Guid.NewGuid(), Name = "Grade 9 - Section A", Code = "GRP001", AcademicYear = 2024, Description = "Advanced Mathematics and Science Group", IsActive = true },
                new Group { Id = Guid.NewGuid(), Name = "Grade 9 - Section B", Code = "GRP002", AcademicYear = 2024, Description = "Standard Curriculum Group", IsActive = true },
                new Group { Id = Guid.NewGuid(), Name = "Grade 10 - Section A", Code = "GRP003", AcademicYear = 2024, Description = "STEM Focus Group", IsActive = true },
                new Group { Id = Guid.NewGuid(), Name = "Grade 10 - Section B", Code = "GRP004", AcademicYear = 2024, Description = "Humanities Focus Group", IsActive = true },
                new Group { Id = Guid.NewGuid(), Name = "Grade 11 - Section A", Code = "GRP005", AcademicYear = 2024, Description = "Advanced Placement Group", IsActive = true },
                new Group { Id = Guid.NewGuid(), Name = "Grade 11 - Section B", Code = "GRP006", AcademicYear = 2024, Description = "College Preparation Group", IsActive = true },
            };

            _context.Groups.AddRange(groups);
        }




        private async Task SeedStudentsAsync()
        {
            if (await _context.Students.AnyAsync())
                return;

            var groups = await _context.Groups.ToListAsync();


            if (groups.Count == 0)
                return;

            var studentEmails = new[]
            {
                ("john.davis@smartcampus.com", "S001"),
                ("emma.miller@smartcampus.com", "S002"),
                ("oliver.wilson@smartcampus.com", "S003"),
                ("sophia.moore@smartcampus.com", "S004"),
                ("liam.taylor@smartcampus.com", "S005"),
                ("ava.anderson@smartcampus.com", "S006"),
                ("noah.thomas@smartcampus.com", "S007"),
                ("isabella.jackson@smartcampus.com", "S008"),
                ("lucas.white@smartcampus.com", "S009"),
                ("mia.harris@smartcampus.com", "S010"),
                ("mason.martin@smartcampus.com", "S011"),
                ("charlotte.garcia@smartcampus.com", "S012"),
                ("ethan.rodriguez@smartcampus.com", "S013"),
                ("amelia.lee@smartcampus.com", "S014"),
                ("logan.perez@smartcampus.com", "S015"),
                ("harper.thompson@smartcampus.com", "S016"),
            };

            var groupIndex = 0;
            foreach (var (email, studentId) in studentEmails)
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user != null && !await _context.Students.AnyAsync(s => s.ApplicationUserId == user.Id))
                {
                    var student = new Student
                    {
                        Id = Guid.NewGuid(),
                        StudentId = studentId,
                        ApplicationUserId = user.Id,
                        GroupId = groups[groupIndex % groups.Count].Id,
                        EnrollmentYear = 2024,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Students.Add(student);
                    groupIndex++;
                }
            }
        }




        private async Task SeedCoursesAsync()
        {
            if (await _context.Courses.AnyAsync())
                return;

            var teachers = await _context.Teachers.ToListAsync();
            var groups = await _context.Groups.ToListAsync();

            if (teachers.Count == 0 || groups.Count == 0)
                return;

            var courses = new[]
            {
                new Course { Id = Guid.NewGuid(), Title = "Introduction to Computer Science", Code = "CS101", Description = "Fundamentals of programming and computer science concepts", TeacherId = teachers[0].Id, GroupId = groups[0].Id, Credits = 4, Semester = "Fall 2024", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Course { Id = Guid.NewGuid(), Title = "Advanced C# & .NET Framework", Code = "CS201", Description = "Deep dive into advanced C# features and .NET ecosystem", TeacherId = teachers[0].Id, GroupId = groups[1].Id, Credits = 4, Semester = "Spring 2025", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Course { Id = Guid.NewGuid(), Title = "Web Development with ASP.NET Core", Code = "CS301", Description = "Build modern web applications using ASP.NET Core", TeacherId = teachers[0].Id, GroupId = groups[2].Id, Credits = 4, Semester = "Fall 2024", IsActive = true, CreatedAt = DateTime.UtcNow },

                new Course { Id = Guid.NewGuid(), Title = "Calculus I - Differential Calculus", Code = "MATH101", Description = "Study of limits, derivatives, and their applications", TeacherId = teachers[1].Id, GroupId = groups[0].Id, Credits = 4, Semester = "Fall 2024", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Course { Id = Guid.NewGuid(), Title = "Calculus II - Integral Calculus", Code = "MATH102", Description = "Integration techniques and applications of integrals", TeacherId = teachers[1].Id, GroupId = groups[1].Id, Credits = 4, Semester = "Spring 2025", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Course { Id = Guid.NewGuid(), Title = "Linear Algebra and Matrices", Code = "MATH201", Description = "Vectors, matrices, eigenvalues, and linear transformations", TeacherId = teachers[1].Id, GroupId = groups[2].Id, Credits = 3, Semester = "Spring 2025", IsActive = true, CreatedAt = DateTime.UtcNow },

                new Course { Id = Guid.NewGuid(), Title = "British Literature", Code = "ENG101", Description = "Exploration of classic British literature from Shakespeare to modern authors", TeacherId = teachers[2].Id, GroupId = groups[3].Id, Credits = 3, Semester = "Fall 2024", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Course { Id = Guid.NewGuid(), Title = "American Literature", Code = "ENG102", Description = "Study of American literary traditions and contemporary works", TeacherId = teachers[2].Id, GroupId = groups[4].Id, Credits = 3, Semester = "Spring 2025", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Course { Id = Guid.NewGuid(), Title = "Creative Writing Seminar", Code = "ENG201", Description = "Develop creative writing skills through workshops and peer review", TeacherId = teachers[2].Id, GroupId = groups[5].Id, Credits = 3, Semester = "Fall 2024", IsActive = true, CreatedAt = DateTime.UtcNow },

                new Course { Id = Guid.NewGuid(), Title = "Physics I - Mechanics", Code = "PHY101", Description = "Classical mechanics including kinematics, dynamics, and energy", TeacherId = teachers[3].Id, GroupId = groups[0].Id, Credits = 4, Semester = "Fall 2024", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Course { Id = Guid.NewGuid(), Title = "Physics II - Thermodynamics", Code = "PHY102", Description = "Heat, temperature, entropy, and thermodynamic processes", TeacherId = teachers[3].Id, GroupId = groups[1].Id, Credits = 4, Semester = "Spring 2025", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Course { Id = Guid.NewGuid(), Title = "Modern Physics", Code = "PHY201", Description = "Relativity, quantum mechanics, and atomic physics fundamentals", TeacherId = teachers[3].Id, GroupId = groups[2].Id, Credits = 4, Semester = "Fall 2024", IsActive = true, CreatedAt = DateTime.UtcNow },

                new Course { Id = Guid.NewGuid(), Title = "General Chemistry I", Code = "CHEM101", Description = "Atomic structure, bonding, and chemical reactions", TeacherId = teachers[4].Id, GroupId = groups[3].Id, Credits = 4, Semester = "Fall 2024", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Course { Id = Guid.NewGuid(), Title = "Organic Chemistry", Code = "CHEM201", Description = "Structure and reactions of organic compounds", TeacherId = teachers[4].Id, GroupId = groups[4].Id, Credits = 4, Semester = "Spring 2025", IsActive = true, CreatedAt = DateTime.UtcNow },

                new Course { Id = Guid.NewGuid(), Title = "World History Since 1500", Code = "HIST101", Description = "Major events and developments in world history from Renaissance to present", TeacherId = teachers[5].Id, GroupId = groups[5].Id, Credits = 3, Semester = "Fall 2024", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Course { Id = Guid.NewGuid(), Title = "American History", Code = "HIST102", Description = "Comprehensive survey of American historical development", TeacherId = teachers[5].Id, GroupId = groups[0].Id, Credits = 3, Semester = "Spring 2025", IsActive = true, CreatedAt = DateTime.UtcNow },
            };

            _context.Courses.AddRange(courses);
        }




        private async Task SeedLessonsAsync()
        {
            if (await _context.Lessons.AnyAsync())
                return;

            var groups = await _context.Groups.ToListAsync();
            var teachers = await _context.Teachers.ToListAsync();

            if (groups.Count == 0 || teachers.Count == 0)
                return;

            var lessons = new List<Lesson>();
            var baseDate = DateTime.Now.AddDays(-30);

            for (int i = 0; i < groups.Count; i++)
            {
                var groupTeacher = teachers[i % teachers.Count];
                for (int j = 1; j <= 3; j++)
                {
                    lessons.Add(new Lesson
                    {
                        Id = Guid.NewGuid(),
                        Title = $"{groups[i].Name} - Lesson {j}",
                        GroupId = groups[i].Id,
                        TeacherId = groupTeacher.Id,
                        LessonNumber = j,
                        LessonDate = baseDate.AddDays(i * 7 + j * 2),
                        Content = $"This is lesson {j} content for {groups[i].Name}. Topics covered: ...",
                        Location = $"Room {100 + i * 10 + j}",
                        IsCompleted = j < 3,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            _context.Lessons.AddRange(lessons);
        }




        private async Task SeedSchedulesAsync()
        {
            if (await _context.Schedules.AnyAsync())
                return;

            var lessons = await _context.Lessons.ToListAsync();
            var teachers = await _context.Teachers.ToListAsync();
            var courses = await _context.Courses.ToListAsync();
            var groups = await _context.Groups.ToListAsync();

            if (lessons.Count == 0 || teachers.Count == 0 || groups.Count == 0)
                return;

            var schedules = new List<Schedule>();
            var daysOfWeek = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" };
            var baseTime = DateTime.Now.Date.AddHours(9);

            for (int i = 0; i < lessons.Count; i++)
            {
                var dayOfWeek = daysOfWeek[i % daysOfWeek.Length];
                var startTime = baseTime.AddHours(i % 5);
                var endTime = startTime.AddHours(1.5);


                var group = groups.FirstOrDefault(g => g.Id == lessons[i].GroupId);
                var course = courses.FirstOrDefault(c => c.GroupId == lessons[i].GroupId);

                schedules.Add(new Schedule
                {
                    Id = Guid.NewGuid(),
                    LessonId = lessons[i].Id,
                    CourseId = course?.Id,
                    TeacherId = lessons[i].TeacherId,
                    GroupId = lessons[i].GroupId,
                    DayOfWeek = dayOfWeek,
                    StartTime = startTime,
                    EndTime = endTime,
                    Location = lessons[i].Location,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }

            _context.Schedules.AddRange(schedules);
        }




        private async Task SeedHomeworkAsync()
        {
            if (await _context.Homework.AnyAsync())
                return;

            var groups = await _context.Groups.ToListAsync();
            var teachers = await _context.Teachers.ToListAsync();

            if (groups.Count == 0 || teachers.Count == 0)
                return;

            var homework = new List<Homework>();
            var today = DateTime.Now;
            var homeworkTypes = new[] { "Task", "Project", "Essay", "Problem Set", "Lab Report" };

            for (int i = 0; i < groups.Count; i++)
            {
                var groupTeacher = teachers[i % teachers.Count];
                for (int j = 0; j < 3; j++)
                {
                    var type = homeworkTypes[j % homeworkTypes.Length];
                    homework.Add(new Homework
                    {
                        Id = Guid.NewGuid(),
                        Title = $"{type} {j + 1} - {groups[i].Code}",
                        Description = $"Complete {type.ToLower()} for group {groups[i].Name}. Requirements: Read the material thoroughly and submit your work on time.",
                        GroupId = groups[i].Id,
                        TeacherId = groupTeacher.Id,
                        CreatedDate = today.AddDays(-(10 + j * 5)),
                        DueDate = today.AddDays(7 + j * 7),
                        Type = type,
                        IsActive = true,
                        IsCompleted = false
                    });
                }
            }

            _context.Homework.AddRange(homework);
        }




        private async Task SeedAnnouncementsAsync()
        {
            if (await _context.Announcements.AnyAsync())
                return;

            var teachers = await _context.Teachers.ToListAsync();
            var courses = await _context.Courses.ToListAsync();

            if (teachers.Count == 0 || courses.Count == 0)
                return;

            var announcements = new List<Announcement>
            {
                new Announcement
                {
                    Id = Guid.NewGuid(),
                    Title = "Welcome to the Course",
                    Content = "Welcome to this course! This is the beginning of an exciting learning journey. I'm looking forward to working with all of you this semester.",
                    TeacherId = teachers[0].Id,
                    CourseId = courses[0].Id,
                    PublishedDate = DateTime.Now.AddDays(-10),
                    IsPublished = true,
                    IsPinned = true
                },
                new Announcement
                {
                    Id = Guid.NewGuid(),
                    Title = "Important: Midterm Exam Schedule",
                    Content = "Midterm exams will be held from December 1-15. Please check your course schedule for exact dates and times. Study materials will be posted by November 20.",
                    TeacherId = teachers[1].Id,
                    CourseId = courses[3].Id,
                    PublishedDate = DateTime.Now.AddDays(-7),
                    IsPublished = true,
                    IsPinned = true
                },
                new Announcement
                {
                    Id = Guid.NewGuid(),
                    Title = "Assignment Deadline Extension",
                    Content = "Due to requests from students, the deadline for Assignment 1 has been extended by 3 days. New deadline: December 5, 2024.",
                    TeacherId = teachers[2].Id,
                    CourseId = courses[6].Id,
                    PublishedDate = DateTime.Now.AddDays(-3),
                    IsPublished = true,
                    IsPinned = false
                },
                new Announcement
                {
                    Id = Guid.NewGuid(),
                    Title = "Lab Session Rescheduled",
                    Content = "The lab session for Physics I has been rescheduled from Friday to Thursday due to building maintenance.",
                    TeacherId = teachers[3].Id,
                    CourseId = courses[8].Id,
                    PublishedDate = DateTime.Now.AddDays(-2),
                    IsPublished = true,
                    IsPinned = false
                },
                new Announcement
                {
                    Id = Guid.NewGuid(),
                    Title = "Office Hours Available",
                    Content = "I will be available for office hours every Tuesday and Wednesday from 2:00 PM to 4:00 PM. Feel free to drop by with any questions.",
                    TeacherId = teachers[4].Id,
                    CourseId = courses[12].Id,
                    PublishedDate = DateTime.Now.AddDays(-1),
                    IsPublished = true,
                    IsPinned = false
                },
                new Announcement
                {
                    Id = Guid.NewGuid(),
                    Title = "Special Guest Lecturer",
                    Content = "We're excited to have Dr. Michael Phillips as our guest lecturer next week. He will be discussing 'Recent Advances in Organic Chemistry'. Class will meet as scheduled.",
                    TeacherId = teachers[4].Id,
                    CourseId = courses[13].Id,
                    PublishedDate = DateTime.Now.AddDays(-1),
                    IsPublished = true,
                    IsPinned = false
                },
                new Announcement
                {
                    Id = Guid.NewGuid(),
                    Title = "Final Project Requirements Posted",
                    Content = "The requirements for the final project have been posted on the course portal. Please review them carefully. Project proposals are due by December 10.",
                    TeacherId = teachers[2].Id,
                    CourseId = courses[7].Id,
                    PublishedDate = DateTime.Now.AddDays(-1),
                    IsPublished = true,
                    IsPinned = false
                },
                new Announcement
                {
                    Id = Guid.NewGuid(),
                    Title = "Reading List Updated",
                    Content = "The reading list for next month has been updated with additional primary sources. All materials are available through the library portal.",
                    TeacherId = teachers[5].Id,
                    CourseId = courses[14].Id,
                    PublishedDate = DateTime.Now.AddDays(-1),
                    IsPublished = true,
                    IsPinned = false
                }
            };

            _context.Announcements.AddRange(announcements);
        }




        private async Task SeedAttendanceRecordsAsync()
        {
            if (await _context.AttendanceRecords.AnyAsync())
                return;

            var students = await _context.Students.ToListAsync();
            var lessons = await _context.Lessons.Take(3).ToListAsync();

            if (students.Count == 0 || lessons.Count == 0)
                return;

            var attendanceRecords = new List<AttendanceRecord>();
            var statuses = new[] { "Present", "Absent", "Late" };

            for (int i = 0; i < lessons.Count; i++)
            {
                for (int j = 0; j < students.Count; j++)
                {
                    attendanceRecords.Add(new AttendanceRecord
                    {
                        Id = Guid.NewGuid(),
                        StudentId = students[j].Id,
                        LessonId = lessons[i].Id,
                        AttendanceDate = lessons[i].LessonDate,
                        Status = statuses[(i + j) % statuses.Length],
                        Remarks = (i + j) % statuses.Length == 2 ? "Arrived 10 minutes late" : null,
                        UpdatedAt = DateTime.UtcNow
                    });
                }
            }

            _context.AttendanceRecords.AddRange(attendanceRecords);
        }




        private async Task SeedGradesAsync()
        {
            if (await _context.Grades.AnyAsync())
                return;

            var students = await _context.Students.ToListAsync();
            var courses = await _context.Courses.ToListAsync();

            if (students.Count == 0 || courses.Count == 0)
                return;

            var grades = new List<Grade>();
            var gradeTypes = new[] { "Homework", "Midterm", "Final", "Project" };
            var random = new Random();


            for (int i = 0; i < students.Count; i++)
            {
                for (int j = 0; j < courses.Count; j++)
                {

                    if (courses[j].GroupId == students[i].GroupId)
                    {
                        int gradeCount = random.Next(2, 4);
                        for (int k = 0; k < gradeCount; k++)
                        {
                            var score = random.Next(60, 101);
                            grades.Add(new Grade
                            {
                                Id = Guid.NewGuid(),
                                StudentId = students[i].Id,
                                CourseId = courses[j].Id,
                                GroupId = students[i].GroupId,
                                Score = score,
                                LetterGrade = GetLetterGrade(score),
                                Feedback = $"Good work on {gradeTypes[k % gradeTypes.Length].ToLower()}",
                                GradeType = gradeTypes[k % gradeTypes.Length],
                                GradedDate = DateTime.UtcNow.AddDays(-random.Next(1, 30)),
                                UpdatedAt = null
                            });
                        }
                    }
                }
            }

            _context.Grades.AddRange(grades);
        }




        private string GetLetterGrade(int score)
        {
            return score switch
            {
                >= 90 => "A",
                >= 80 => "B",
                >= 70 => "C",
                >= 60 => "D",
                _ => "F"
            };
        }
    }
}