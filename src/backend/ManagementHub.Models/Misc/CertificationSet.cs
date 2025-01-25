using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ManagementHub.Models.Domain.Tests;
using ManagementHub.Models.Enums;

namespace ManagementHub.Models.Misc;
public class CertificationSet : ISet<Certification>
{
	private readonly bool[] certifications;
	private static readonly int Levels = Enum.GetValues<CertificationLevel>().Length;
	private static readonly int Versions = Enum.GetValues<CertificationVersion>().Length;
	public CertificationSet()
	{
		this.certifications = new bool[Levels * Versions];
	}
	public int Count => this.certifications.Count(static x => x);

	public bool IsReadOnly => false;

	private static int GetIndex(Certification item) => LevelIndexMap(item.Level) * Versions + (int)item.Version;

	private static int LevelIndexMap(CertificationLevel level) => level switch
	{
		CertificationLevel.Field => 4,
		CertificationLevel.Head => 3,
		CertificationLevel.Flag => 2,
		CertificationLevel.Assistant => 1,
		CertificationLevel.Scorekeeper => 0,
		_ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
	};

	private static CertificationLevel LevelIndexMap(int index) => index switch
	{
		4 => CertificationLevel.Field,
		3 => CertificationLevel.Head,
		2 => CertificationLevel.Flag,
		1 => CertificationLevel.Assistant,
		0 => CertificationLevel.Scorekeeper,
		_ => throw new ArgumentOutOfRangeException(nameof(index), index, null)
	};

	public bool Add(Certification item)
	{
		bool inSet = this.certifications[GetIndex(item)];
		this.certifications[GetIndex(item)] = true;
		return !inSet;
	}

	public void Clear()
	{
		for (int i = 0; i < this.certifications.Length; i++)
		{
			this.certifications[i] = false;
		}
	}

	public bool Contains(Certification item)
	{
		return this.certifications[GetIndex(item)];
	}

	public void CopyTo(Certification[] array, int arrayIndex)
	{
		for (int i = 0; i < this.certifications.Length; i++)
		{
			if (this.certifications[i])
			{
				array[arrayIndex++] = new Certification(LevelIndexMap(i / Levels), (CertificationVersion)(i % Levels));
			}
		}
	}

	public void ExceptWith(IEnumerable<Certification> other)
	{
		foreach (var item in other)
		{
			this.certifications[GetIndex(item)] = false;
		}
	}

	IEnumerator<Certification> IEnumerable<Certification>.GetEnumerator()
	{
		return new Enumerator(this);
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}

	public void IntersectWith(IEnumerable<Certification> other)
	{
		foreach (var item in other)
		{
			if (!this.certifications[GetIndex(item)])
			{
				this.certifications[GetIndex(item)] = false;
			}
		}
	}

	public bool IsProperSubsetOf(IEnumerable<Certification> other)
	{
		throw new NotImplementedException();
	}

	public bool IsProperSupersetOf(IEnumerable<Certification> other)
	{
		throw new NotImplementedException();
	}

	public bool IsSubsetOf(IEnumerable<Certification> other)
	{
		throw new NotImplementedException();
	}

	public bool IsSupersetOf(IEnumerable<Certification> other)
	{
		throw new NotImplementedException();
	}

	public bool Overlaps(IEnumerable<Certification> other)
	{
		foreach (var item in other)
		{
			if (this.certifications[GetIndex(item)])
			{
				return true;
			}
		}
		return false;
	}

	public bool Remove(Certification item)
	{
		bool inSet = this.certifications[GetIndex(item)];
		this.certifications[GetIndex(item)] = false;
		return inSet;
	}

	public bool SetEquals(IEnumerable<Certification> other)
	{
		for (int i = 0; i < this.certifications.Length; i++)
		{
			if (this.certifications[i] != other.Contains(new Certification(LevelIndexMap(i / Levels), (CertificationVersion)(i % Levels))))
			{
				return false;
			}
		}
		return true;
	}

	public void SymmetricExceptWith(IEnumerable<Certification> other)
	{
		throw new NotImplementedException();
	}

	public void UnionWith(IEnumerable<Certification> other)
	{
		foreach (var item in other)
		{
			this.certifications[GetIndex(item)] = true;
		}
	}

	void ICollection<Certification>.Add(Certification item)
	{
		this.Add(item);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return new Enumerator(this);
	}

	public struct Enumerator : IEnumerator<Certification>
	{
		private readonly CertificationSet set;
		private int currentLevel;
		private int currentVersion;
		public Enumerator(CertificationSet set)
		{
			this.set = set;
			this.Reset();
		}
		public Certification Current => new Certification(LevelIndexMap(this.currentLevel), (CertificationVersion)this.currentVersion);
		object IEnumerator.Current => this.Current;
		public void Dispose()
		{
		}
		public bool MoveNext()
		{
			if (this.currentLevel == -1)
			{
				return false;
			}

			do
			{
				if (this.currentVersion == 0)
				{
					this.currentVersion = Versions - 1;
					this.currentLevel--;
				}
				else
				{
					this.currentVersion--;
				}

				if (this.currentLevel == -1)
				{
					return false;
				}
			}
			while (this.set.certifications[GetIndex(this.Current)] == false);

			return true;
		}
		public void Reset()
		{
			this.currentLevel = Levels;
			this.currentVersion = 0;
		}
	}
}
