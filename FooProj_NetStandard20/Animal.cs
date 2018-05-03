namespace FooProj_NetStandard20
{
	public enum AnimalType { Cat, Dog, Zebra, Alligator }

	public class Animal
	{
		public string Name { get; set; }

		public AnimalType AnimalType { get; set; }

		public int Age { get; set; }

		public string GetInfo() => $"Animal name is: {Name}, of type {AnimalType}, aged {Age}";

		public static int AddEm(int a, int b) => a + b;
	}
}
