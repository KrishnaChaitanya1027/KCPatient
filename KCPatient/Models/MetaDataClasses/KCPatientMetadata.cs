using KCClassLibrary;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KCPatient.Models
{
    [ModelMetadataType(typeof(KCPatientMetadata))]
    public partial class Patient : IValidatableObject
    {
       
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            FirstName = KCValidations.KCCapitalize(FirstName);
            LastName = KCValidations.KCCapitalize(LastName);
            Address = KCValidations.KCCapitalize(Address);
            City = KCValidations.KCCapitalize(City);
            Gender = KCValidations.KCCapitalize(Gender);
            ProvinceCode = ProvinceCode?.ToUpper();

            if  ( string.IsNullOrEmpty(ProvinceCode))
                yield return new ValidationResult("Province code is required to validate Postal Code",
                                                    new[] { nameof(ProvinceCode) });

            if ( DateOfBirth.HasValue && IsInValidDate(DateOfBirth.Value))
                yield return new ValidationResult("Date of Birth cannot be in the future",
                                                    new[] { nameof(DateOfBirth) });

            if ( !string.IsNullOrEmpty(HomePhone) && IsPhoneNumInvalid(HomePhone))
                yield return new ValidationResult("Home Phone must be 10 digits: 123-123-1234",
                                                    new[] { nameof(HomePhone) });

            if (!string.IsNullOrEmpty(Gender) && (Gender != "M" && Gender != "F" && Gender != "X" ))
                yield return new ValidationResult("Gender must  be either 'M', 'F' or 'X'" ,
                                                    new[] { nameof(Gender)});

            if (!string.IsNullOrEmpty(Ohip) && IsOHIPInvalid(Ohip))
                yield return new ValidationResult("OHIP, if provided must match patter 1234-123-123-XX",
                                                    new[] { nameof(Ohip) });

            if (!string.IsNullOrEmpty(PostalCode) && KCValidations.KCPostalCodeValidation(PostalCode))
            {
                yield return new ValidationResult("Postal Code not cdn pattern: A3A 3A3",
                                                   new[] { nameof(PostalCode) });
            }
            
               

            if (!string.IsNullOrEmpty(PostalCode)  && !string.IsNullOrEmpty(ProvinceCode) && CheckFirstLetter(PostalCode))
                yield return new ValidationResult("First letter of Postal Code not valid for given province",
                                                    new[] { nameof(PostalCode) });

            if (!string.IsNullOrEmpty(ProvinceCode) && CheckProvinceCode(ProvinceCode))
                yield return new ValidationResult("Province code is not on file",
                                                    new[] { nameof(ProvinceCode) });

            if (Deceased)
            {
                if(!DateOfDeath.HasValue)
                {
                    yield return new ValidationResult("If deceased is true, dateOfDeath is required",
                                                 new[] { nameof(DateOfDeath) });
                }

                if (DateOfDeath.HasValue && (IsInValidDate(DateOfDeath.Value) && CheckBeforeBirth(DateOfDeath.Value) ))
                {
                    yield return new ValidationResult("Date of death cannot be in the future",
                                                  new[] { nameof(DateOfDeath) });
                }

               
            }

            if (!Deceased)
            {
               if(DateOfDeath.HasValue)
                {
                    yield return new ValidationResult("Deceased must be true if date of death is provided",
                                                  new[] { nameof(Deceased) });
                }
               
            }

    

            yield return ValidationResult.Success;
        }

        bool CheckFirstLetter(string text)
        {
            using (var patientContext = new PatientContext())
            {
                var provinceData = patientContext.Province.Where(x => x.ProvinceCode.ToLower() == ProvinceCode.ToLower()).FirstOrDefault();
                if (provinceData != null )
                {
                    if(provinceData.CountryCode=="CA" && provinceData.FirstPostalLetter.ToLower() != text.Substring(0, 1).ToLower())
                    {
                        return true;
                        
                    }
                    else
                    {
                        if(provinceData.CountryCode == "CA")
                        {
                            PostalCode = KCValidations.KCPostalCodeFormat(PostalCode);
                        }
                        else
                        {
                            KCValidations.KCZipCodeValidation(PostalCode);
                        }

                        return false;
                    }
                             
                }
                else
                {
                    return false;
                }
            }

        }

        bool CheckProvinceCode(string text)
        {

            using (var patientContext = new PatientContext())
            {
                var provinceData = patientContext.Province.Where(x => x.ProvinceCode.ToLower() == ProvinceCode.ToLower()).FirstOrDefault();
                if (provinceData == null )
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }



        }

        bool IsOHIPInvalid(string text)
        {
            var regex = new Regex(@"^[0-9]{4}[-][0-9]{3}[-][0-9]{3}-[A-Z]{2}$");

            if ((!regex.IsMatch(text)))
                return true;

            text.ToUpper();
            return false;
        }

        bool CheckBeforeBirth(DateTime deathDate)
        {
            if (DateOfBirth.HasValue && DateOfBirth.Value > deathDate)
            {
                return true;
            }
            else {
                return false;
            }
        }

        bool IsInValidDate(DateTime date)
        {
            if(date > DateTime.Now)
            {
                return true;
            }
            return false;
            
        }

        bool IsPhoneNumInvalid(string text)
        {
     
            var regex = new Regex(@"^\d{10}$");

            if ((!regex.IsMatch(text)))
                return true;
            return false;
        }


    }

    public class KCPatientMetadata
    {
        [Display(Name = "First Name")]
        [Required(ErrorMessage = "First Name is required.")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        [Required(ErrorMessage = "Last Name is required.")]
        public string LastName { get; set; }

        [Display(Name = "Street Address")]
        public string Address { get; set; }

        [Display(Name = "City")]
        public string City { get; set; }

        [Display(Name = "Province Code")]

        public string ProvinceCode { get; set; }

        [Display(Name = "Postal Code")]

        public string PostalCode { get; set; }

        [Display(Name = "OHIP")]
        public string Ohip { get; set; }

        [Display(Name = "Date of Birth")]
        public DateTime? DateOfBirth { get; set; }
        public bool Deceased { get; set; }

        [Display(Name = "Date of Death")]

        public DateTime? DateOfDeath { get; set; }

        [Display(Name = "Home Phone")]
        public string HomePhone { get; set; }

        [Required(ErrorMessage = "Gender is required.")]
        public string Gender { get; set; }

    }
}
