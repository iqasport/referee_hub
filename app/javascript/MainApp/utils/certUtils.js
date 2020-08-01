import { DateTime } from 'luxon'

export function hasPassedTest(level, refCertifications, levelsThatNeedRenewal) {
  const passedCert = refCertifications
    && refCertifications.some(({ level: certificationLevel }) => certificationLevel === level)
  const levelNeedsRenewal = levelsThatNeedRenewal && levelsThatNeedRenewal.find(details => details.level === level)

  if (levelNeedsRenewal) return false
  return passedCert
}

export function isInCoolDownPeriod(certType, testAttempts) {
  const matchingTestAttempt = testAttempts.filter(testAttempt => testAttempt.test_level === certType)

  if (matchingTestAttempt.length > 0) {
    const rawAttemptString = matchingTestAttempt[0].next_attempt_at.slice(0, -3).trim()
    const nextAttemptAt = DateTime.fromSQL(rawAttemptString)
    const currentTime = DateTime.local()

    return !(nextAttemptAt < currentTime)
  }

  return false
}

export function hasSnitchCert(refCertifications, levelsThatNeedRenewal) {
  return hasPassedTest('snitch', refCertifications, levelsThatNeedRenewal)
}

export function hasAssistantCert(refCertifications, levelsThatNeedRenewal) {
  return hasPassedTest('assistant', refCertifications, levelsThatNeedRenewal)
}

export function hasHeadCert(refCertifications, levelsThatNeedRenewal) {
  return hasPassedTest('head', refCertifications, levelsThatNeedRenewal)
}

export function canTakeSnitchTest(levelsThatNeedRenewal, testAttempts, refCertifications) {
  if (isInCoolDownPeriod('snitch', testAttempts)) return false
  if (levelsThatNeedRenewal.find(refCert => refCert.level === 'snitch')) return true

  return !hasSnitchCert(refCertifications, levelsThatNeedRenewal)
    && hasAssistantCert(refCertifications, levelsThatNeedRenewal)
    && !hasHeadCert(refCertifications, levelsThatNeedRenewal)
}

export function canTakeAssistantTest(levelsThatNeedRenewal, testAttempts, refCertifications) {
  if (isInCoolDownPeriod('assistant', testAttempts)) return false
  if (levelsThatNeedRenewal.find(refCert => refCert.level === 'assistant')) return true

  return !hasAssistantCert(refCertifications, levelsThatNeedRenewal)
    && !hasHeadCert(refCertifications, levelsThatNeedRenewal)
}

export function canTakeHeadTest(hasPaid, levelsThatNeedRenewal, testAttempts, refCertifications) {
  if (!hasPaid) return false
  if (isInCoolDownPeriod('head', testAttempts)) return false
  if (levelsThatNeedRenewal.find(refCert => refCert.level === 'head')) return true

  return hasSnitchCert(refCertifications, levelsThatNeedRenewal)
    && hasAssistantCert(refCertifications, levelsThatNeedRenewal)
}
