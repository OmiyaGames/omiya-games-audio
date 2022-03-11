using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace OmiyaGames.Common.Editor
{
	public class RangeSlider : MinMaxSlider
	{
		const float FIELD_FLEX_GROW = 0.3f;
		const float MIN_FIELD_WIDTH = 20f;
		const float FIELD_PADDING = 8f;
		/// <summary>
		/// TODO
		/// </summary>
		public static readonly string minFieldUssClassName = ussClassName + "__min-field";
		/// <summary>
		/// TODO
		/// </summary>
		public static readonly string maxFieldUssClassName = ussClassName + "__max-field";

		/// <summary>
		/// <seealso cref="UxmlFactory{TCreatedType, TTraits}"/> for <see cref="RangeSlider"/>.
		/// </summary>
		public new class UxmlFactory : UxmlFactory<RangeSlider, UxmlTraits> { }

		FloatField minField
		{
			get;
		}
		FloatField maxField
		{
			get;
		}

		/// <summary>
		/// TODO
		/// </summary>
		public RangeSlider() : this(null) { }

		/// <summary>
		/// TODO
		/// </summary>
		/// <param name="minValue"></param>
		/// <param name="maxValue"></param>
		/// <param name="minLimit"></param>
		/// <param name="maxLimit"></param>
		public RangeSlider(float minValue, float maxValue, float minLimit, float maxLimit) : this(null, minValue, maxValue, minLimit, maxLimit) { }

		/// <summary>
		/// TODO
		/// </summary>
		/// <param name="label"></param>
		/// <param name="minValue"></param>
		/// <param name="maxValue"></param>
		/// <param name="minLimit"></param>
		/// <param name="maxLimit"></param>
		public RangeSlider(string label, float minValue = 0, float maxValue = 10, float minLimit = -10, float maxLimit = 20) : base(label, minValue, maxValue, minLimit, maxLimit)
		{
			// Setup the float fields
			minField = new FloatField()
			{
				name = "min-field"
			};
			maxField = new FloatField()
			{
				name = "max-field"
			};

			// Add classes
			minField.AddToClassList(minFieldUssClassName);
			maxField.AddToClassList(maxFieldUssClassName);

			// Setup field styles
			// TODO: verify if this can be overwritten by classes
			minField.style.flexBasis = 0f;
			minField.style.flexGrow = FIELD_FLEX_GROW;
			minField.style.minWidth = MIN_FIELD_WIDTH;
			minField.style.paddingRight = FIELD_PADDING;
			maxField.style.flexBasis = 0f;
			maxField.style.flexGrow = FIELD_FLEX_GROW;
			maxField.style.minWidth = MIN_FIELD_WIDTH;
			maxField.style.paddingLeft = FIELD_PADDING;

			// Setup their initial values
			minField.value = minValue;
			maxField.value = maxValue;

			// Bind events
			minField.RegisterCallback<ChangeEvent<float>>(e => base.minValue = e.newValue);
			maxField.RegisterCallback<ChangeEvent<float>>(e => base.maxValue = e.newValue);

			// Insert the fields
			contentContainer.Insert(0, minField);
			contentContainer.Add(maxField);
		}

		/// <inheritdoc/>
		public override Vector2 value
		{
			get => base.value;
			set
			{
				// Update slider
				base.value = value;

				// Update the text fields
				minField?.SetValueWithoutNotify(base.value.x);
				maxField?.SetValueWithoutNotify(base.value.y);
			}
		}

		/// <inheritdoc/>
		public override void SetValueWithoutNotify(Vector2 newValue)
		{
			// Update slider
			base.SetValueWithoutNotify(newValue);

			// Update the text fields
			minField?.SetValueWithoutNotify(newValue.x);
			maxField?.SetValueWithoutNotify(newValue.y);
		}
	}
}
