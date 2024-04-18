using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace backend.Models
{
    public class WorkoutModel
    {

        public int Id { get; set; }
        [Required(ErrorMessage = "Muscle Group is Required.")]
        public string? MuscleGroup { get; set; }
        [Required(ErrorMessage = "Exercise is Required.")]
        public string? Exercise { get; set; }
        [Required(ErrorMessage = "Number of Sets is Required.")]
        public int SetNumber { get; set; }
        [Required(ErrorMessage = "Number of Reps is Required.")]
        public int RepNumber { get; set; }
        public string? UserId { get; set; }


        public DateTime DateTime { get; set; }



    }



}