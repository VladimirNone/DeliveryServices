package com.example.deliverymanapp

import android.content.Context
import android.content.Intent
import android.content.SharedPreferences
import android.os.Bundle
import android.text.Editable
import android.view.View
import android.widget.*
import androidx.appcompat.app.AppCompatActivity
import com.android.volley.Request
import com.android.volley.RequestQueue
import com.android.volley.toolbox.StringRequest
import com.android.volley.toolbox.Volley
import com.example.deliverymanapp.dto.OrderItemDTO
import com.example.deliverymanapp.dto.OrderStateItemDTO
import com.example.deliverymanapp.dto.OrdersDTO
import com.google.android.gms.location.*
import com.google.gson.Gson
import org.json.JSONException
import java.net.CookieHandler
import java.net.CookieManager


class OrderListActivity : AppCompatActivity() {
    private var mRequestQueue: RequestQueue? = null
    private var sharedPreferences :SharedPreferences? = null
    private var states = arrayOf<OrderStateItemDTO>()
    private val gson: Gson = Gson()
    private var url:String = ""
    private var orders = mutableListOf<OrderItemDTO>()
    private var page = 0
    private var lastPage = true
    private var selectedState: OrderStateItemDTO? = null
    private var statesAdapter:ArrayAdapter<String>? = null

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_order_list)

        mRequestQueue = Volley.newRequestQueue(this)
        sharedPreferences = getSharedPreferences("my_preference", Context.MODE_PRIVATE)
        url = resources.getString(R.string.url)

        fetchListOfStates()
    }

    private fun fetchListOfStates() {

        val req = StringRequest(
            Request.Method.GET, "${url}/main/getOrderStates",
            { response ->
                try {
                    states = gson.fromJson(response, Array<OrderStateItemDTO>::class.java)

                    statesAdapter = ArrayAdapter(this, R.layout.dropdown_item, states.map { it.nameOfState })
                    val headerView = findViewById<View>(R.id.header)

                    val autocompleteTV = headerView.findViewById<AutoCompleteTextView>(R.id.autoCompleteTextView)

                    autocompleteTV.setAdapter(statesAdapter)

                } catch (e: JSONException) {
                    e.printStackTrace()
                }
            },
            { error ->
                run {
                    Toast.makeText(this@OrderListActivity, "Bad request", Toast.LENGTH_SHORT).show()
                }
            })

        /* Add your Requests to the RequestQueue to execute */
        mRequestQueue!!.add(req)
    }

    fun selectOrderState(view: View) {
        val textView:TextView = view as TextView
        selectedState = states.find { it.nameOfState == textView.text }

        if(selectedState != null){
            orders.clear()
            loadAndAddOrdersToList()
        }

        val headerView = findViewById<View>(R.id.header)

        val autocompleteTV = headerView.findViewById<AutoCompleteTextView>(R.id.autoCompleteTextView)
        autocompleteTV.clearFocus()

//        autocompleteTV.text = Editable.Factory.getInstance().newEditable(selectedState?.nameOfState)
//        autocompleteTV.setAdapter(statesAdapter)
    }

    private fun loadAndAddOrdersToList(){
        val req = object : StringRequest(
            Request.Method.GET, "${url}/DeliveryMan/getOrders?page=${page}&numberOfState=${selectedState?.numberOfStage}",
            { response ->
                try {
                    val ordersInput = gson.fromJson(response, OrdersDTO::class.java)
                    orders.addAll(ordersInput.orders)
                    lastPage = ordersInput.pageEnded

                    if(selectedState != null) {
                        val res:Boolean = (selectedState!!.numberOfStage == 4 || selectedState!!.numberOfStage == 8)
                        orders.forEach{it.canDelManChangestate = res}
                    }

                    val showMoreButton = findViewById<Button>(R.id.showMoreButton)
                    if(lastPage){
                        showMoreButton.visibility = View.INVISIBLE
                    }
                    else{
                        showMoreButton.visibility = View.VISIBLE
                    }

                    val arrayAdapter = AdapterOrders(this, R.layout.order_item, orders.toList())
                    val listView = findViewById<ListView>(R.id.orderList)

                    listView.adapter = arrayAdapter

                } catch (e: JSONException) {
                    e.printStackTrace()
                }
            },
            { error ->
                run {
                    Toast.makeText(this@OrderListActivity, "Bad request", Toast.LENGTH_SHORT).show()
                }
            })
        {
            override fun getHeaders(): MutableMap<String, String> {
                val headers = HashMap<String, String>()
                headers["Authorization"] = "Bearer " + sharedPreferences?.getString("jwtToken", "")
                return headers
            }
        }

        /* Add your Requests to the RequestQueue to execute */
        mRequestQueue!!.add(req)
    }

    fun showMoreOrdersClick(view: View) {
        loadAndAddOrdersToList()
    }

    fun logoutClick(view: View) {
        val cookieManager = CookieManager()
        CookieHandler.setDefault(cookieManager)

        cookieManager.cookieStore?.removeAll()

        val sharedPreferences = getSharedPreferences("my_preference", Context.MODE_PRIVATE)
        val editor = sharedPreferences?.edit()
        editor?.remove("X-UserId")
        editor?.remove("X-Refresh-Token")
        editor?.apply()

        val intent = Intent(this, MainActivity::class.java)
        this.startActivity(intent);
    }
}

