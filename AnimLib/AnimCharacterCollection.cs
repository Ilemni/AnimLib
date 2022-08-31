using System.Collections;
using System.Collections.Generic;
using AnimLib.Internal;
using JetBrains.Annotations;
using Terraria.ModLoader;

namespace AnimLib {
  internal class AnimCharacterCollection : IReadOnlyDictionary<Mod, AnimCharacter> {
    private readonly CharStack<AnimCharacter> characterStack;
    private AnimCharacter.Priority activePriority;

    internal AnimCharacterCollection(AnimPlayer animPlayer) {
      if (AnimLoader.GetLoadedMods(out var mods)) {
        foreach (Mod mod in mods) this[mod] = new AnimCharacter(animPlayer, mod);

        characterStack = new CharStack<AnimCharacter>(mods.Count);
      }
    }

    internal Dictionary<Mod, AnimCharacter> dict { get; } = new Dictionary<Mod, AnimCharacter>();
    [CanBeNull] public AnimCharacter ActiveCharacter { get; private set; }

    public bool ContainsKey(Mod key) => dict.ContainsKey(key);
    public bool TryGetValue(Mod key, out AnimCharacter value) => dict.TryGetValue(key, out value);

    public AnimCharacter this[Mod mod] {
      get => dict[mod];
      private set => dict[mod] = value;
    }

    public IEnumerable<Mod> Keys => dict.Keys;
    public IEnumerable<AnimCharacter> Values => dict.Values;


    public IEnumerator<KeyValuePair<Mod, AnimCharacter>> GetEnumerator() => dict.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => dict.Count;

    public bool CanEnable(AnimCharacter.Priority priority = AnimCharacter.Priority.Default) {
      if (ActiveCharacter == null) return true;
      return activePriority switch
      {
         AnimCharacter.Priority.Lowest => true,
         _ => priority > activePriority,
      };
    }

    /// <summary>
    /// Enables the given <see cref="AnimCharacter"/> with the given <see cref="AnimCharacter.Priority"/>.
    /// If there was an <see cref="ActiveCharacter"/>, it will be disabled and put into the character stack.
    /// </summary>
    /// <param name="character"></param>
    /// <param name="priority"></param>
    internal void Enable([NotNull] AnimCharacter character, AnimCharacter.Priority priority) {
      AnimCharacter previous = ActiveCharacter;
      if (previous is not null) {
        previous.Disable();
        // Set stack position of previous active char to most recent.
        characterStack.TryRemove(previous);
        characterStack.Push(previous);
      }


      ActiveCharacter = character;
      characterStack.TryRemove(character);
      ActiveCharacter.Enable();
      activePriority = priority;
    }

    /// <summary>
    /// Disable the given <see cref="AnimCharacter"/>.
    /// If <paramref name="character"/> was <see cref="ActiveCharacter"/>, <see cref="ActiveCharacter"/> will be replaced with the next character in the stack.
    /// </summary>
    /// <param name="character">The <see cref="AnimCharacter"/> to disable.</param>
    internal void Disable([NotNull] AnimCharacter character) {
      characterStack.TryRemove(character);
      if (character == ActiveCharacter) ActiveCharacter = characterStack.Pop();
    }
  }

  internal class CharStack<T> {
    private readonly List<T> items;
    public CharStack() => items = new List<T>();
    public CharStack(int count) => items = new List<T>(count);

    public int Count => items.Count;

    public void Push(T item) => items.Add(item);

    public T Pop() {
      if (items.Count <= 0) return default;
      T temp = items[items.Count - 1];
      items.RemoveAt(items.Count - 1);
      return temp;
    }

    public bool Contains(T item) => items.IndexOf(item) >= 0;

    public void Remove(int itemAtPosition) => items.RemoveAt(itemAtPosition);

    public void TryRemove(T item) {
      int index = items.IndexOf(item);
      if (index >= 0) Remove(index);
    }
  }
}
