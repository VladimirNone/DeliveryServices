package com.example.deliverymanapp

import android.content.Context
import android.content.Intent
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.ArrayAdapter
import android.widget.Button
import android.widget.TextView
import android.widget.Toast
import com.example.deliverymanapp.dto.OrderItemDTO

class AdapterOrders (context: Context, private val resource: Int, private var orders:List<OrderItemDTO>) : ArrayAdapter<OrderItemDTO>(context, resource, orders) {
    private var inflater: LayoutInflater = LayoutInflater.from(context)

    override fun getView(position: Int, convertView: View?, parent: ViewGroup): View {
        val view: View = inflater.inflate(this.resource, parent, false)

        val tvOrderId = view.findViewById<TextView>(R.id.OrderId)
        val tvPrice = view.findViewById<TextView>(R.id.Price)
        val tvWeight = view.findViewById<TextView>(R.id.Weight)
        val tvAddress = view.findViewById<TextView>(R.id.Address)

        tvOrderId.text = "Id: ${orders[position].id}"
        tvPrice.text = "Стоимость: ${orders[position].price} Р"
        tvWeight.text = "Вес: ${orders[position].sumWeight}"
        tvAddress.text = "Адрес: ${orders[position].deliveryAddress}"

        view.setOnClickListener{ v -> run{
            Toast.makeText(context, "Clicked item :"+" "+position, Toast.LENGTH_SHORT).show()
            val intent = Intent(context, OrderActivity::class.java)
            intent.putExtra("orderId", orders[position].id);
            context.startActivity(intent);
        }}


        return view
    }
}