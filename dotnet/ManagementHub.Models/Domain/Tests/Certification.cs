using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ManagementHub.Models.Enums;

namespace ManagementHub.Models.Domain.Tests;
public record struct Certification(CertificationLevel Level, CertificationVersion Version);
